using System.Collections.Immutable;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using Intellisense;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents;

public class CompletionManager : IDisposable
{
    private readonly TextEditor _textEditor;
    private readonly IEditorCursorViewModel _editorHelper;
    private readonly ICompletionManagerServiceLocator _vm;
    private readonly CompletionWindowWrapper _completionWindow;

    public CompletionManager(TextEditor editor,
        IEditorCursorViewModel editorHelper,
        ICompletionManagerServiceLocator vm,
        CompletionWindowWrapper completionWindow
        )
    {
        _textEditor = editor;
        _editorHelper = editorHelper;
        _vm = vm;
        _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        _completionWindow = completionWindow;
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    // ReSharper disable once AsyncVoidMethod
    private async void Caret_PositionChanged(object? sender, EventArgs eventArgs)
    {
        try
        {
            await _vm._intellisenseClient.CancelRequestAsync();
        }
        catch (IntellisenseException exc)
        {
            _vm._logger.LogWarning(exc, "Intellisense exception occurred");
        }
        catch (OperationCanceledException exc)
        {
            _vm._logger.LogDebug(exc, "Intellisense request cancelled");
        }
    }

    private async Task<bool> ShowPathCompletions()
    {
        // Avoid unnecessary IO calls
        if (_completionWindow.IsOpen) return false;

        var currentLineText = _editorHelper.GetCurrentLineText();

        var args = CommandLineStringSplitter.Instance.Split(currentLineText).ToArray();

        if (args.Length < 2)
        {
            return false;
        }

        var lastArg = args[^1];
        var command = args[0];

        // check if it starts with a valid file IO command like ".save"
        if (!_vm._allowedCommandsAndExtensions.TryGetValue(command, out var extensions))
        {
            return false;
        }


        var result = CompletionResult.Empty;
        try
        {
            // TODO: discreetly notify user (status bar? notifications inbox?) to check connection status of saved connections
            // and user profile app was started with if hosts don't show shares
            result = await _vm._intellisenseClient.GetCompletionResultAsync(lastArg);

        }
        catch (IntellisenseException exc)
        {
            _vm._logger.LogWarning(exc, "Intellisense exception occurred");
        }
        catch (OperationCanceledException exc)
        {
            _vm._logger.LogDebug(exc, "Intellisense request cancelled");
        }

        if (result.IsEmpty()) return false;

        // filter out files without valid extensions
        // permit all file types if allowlist is empty

        if (extensions.Count > 0)
        {
            result = result with
            {
                Entries = result.Entries.Where(x =>
                {
                    // permit folders (which do not have extensions). note that files without extensions will still be allowed
                    var ext = Path.GetExtension(x.Name);
                    return ext == string.Empty || extensions.Contains(ext);
                }).ToList()
            };
        }

        _completionWindow.ShowCompletions(new ShowCompletionOptions
        {
            Completions = result.Entries,
            Rewind = result.Filter.Length,
            OnCompletionWindowDataPopulated = completionWindow =>
            {
                // when editor loses focus mid-path and user resumes typing,
                // it won't require a second keypress to select the relevant result
                completionWindow.CompletionList.SelectItem(result.Filter);
                if (!completionWindow.CompletionList.CurrentList.Any())
                    completionWindow.Close();
            }
        });


        return true;


    }

    public async Task HandleKeyDown(TextInputEventArgs e)
    {
        _completionWindow.CloseIfEmpty();

        if (await ShowPathCompletions())
            return;

        var options = e.Text switch
        {
            "." when _editorHelper.TextToLeftOfCaret().TrimStart() is "." => new()
            {
                Completions = _vm.InternalCommands
            },
            "|" => new()
            {
                Completions = _vm.KqlOperatorEntries,
                Prefix = " "
            },
            "@" => new()
            {
                Completions = _vm.GetColumns(_editorHelper.GetTextAroundCursor()),
                Rewind = 1
            },
            "[" => new()
            {
                Completions = _vm.GetTables(_editorHelper.GetTextAroundCursor()),
                Rewind = 1
            },
            "$" => new()
            {
                Completions = _vm.SettingNames
            },
            "?" => new()
            {
                Completions = _vm.KqlFunctionEntries,
                Rewind = 1
            },
            _ => ShowCompletionOptions.Empty
        };

        _completionWindow.ShowCompletions(options);

    }


    public void Dispose()
    {
        _textEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        _completionWindow.Close();

    }
}

public class CompletionWindowWrapper(TextArea textArea)
{
    private CompletionWindow? _completionWindow;

    public bool IsOpen => _completionWindow?.IsOpen ?? false;

    public IEnumerable<string> GetCurrentCompletionListEntries()
    {
        return _completionWindow?.CompletionList.CurrentList.Select(x => x.Text) ?? ImmutableArray<string>.Empty;
    }

    public void CloseIfEmpty()
    {
        if (_completionWindow != null && !GetCurrentCompletionListEntries().Any())
        {
            _completionWindow.Close();
            // return;
        }
    }

    public void Close()
    {
        if (_completionWindow is not { IsOpen:false } c) return;
        c.Close();

        _completionWindow = null;
    }

    public void ShowCompletions(ShowCompletionOptions options)
    {
        var completions = options.Completions;
        var prefix = options.Prefix;
        var rewind = options.Rewind;
        var onCompletionWindowDataPopulated = options.OnCompletionWindowDataPopulated;
        if (!completions.Any())
            return;

        _completionWindow = new CompletionWindow(textArea)
        {
            CloseWhenCaretAtBeginning = true,
            MaxWidth = 200
        };
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
        foreach (var k in completions.OrderBy(k => k.Name))
            data.Add(new QueryEditorCompletionData(k, prefix, rewind));

        _completionWindow.Closed += delegate { _completionWindow = null; };
        onCompletionWindowDataPopulated.Invoke(_completionWindow);
        _completionWindow?.Show();
    }

}

public readonly record struct ShowCompletionOptions()
{
    public IReadOnlyCollection<IntellisenseEntry> Completions { get; init; } = [];
    public string Prefix { get; init; } = string.Empty;
    public int Rewind { get; init; }
    public Action<CompletionWindow> OnCompletionWindowDataPopulated { get; init; } = EmptyAction;
    private static readonly Action<CompletionWindow> EmptyAction = _ => { };
    public static ShowCompletionOptions Empty => new();
}
