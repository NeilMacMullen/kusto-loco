using System.Diagnostics.CodeAnalysis;
using Avalonia.Input;
using AvaloniaEdit;
using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.ViewModels;

namespace lokqlDxComponents.Views.Dialogs;

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
    private async void Caret_PositionChanged(object? sender, EventArgs eventArgs) => await _vm._intellisenseClient.OnCaretPositionChanged();

    private ShowCompletionOptions CreateShowCompletionOptions(CompletionRequest completionRequest) => new()
    {
        Completions = completionRequest
            .Completions.OrderBy(x => x.Name)
            .Select(entry =>
                new QueryEditorCompletionData(entry, completionRequest.Prefix, completionRequest.Rewind)
                {
                    Image = _vm._intellisenseClient._imageProvider.GetImage(entry.Hint)
                }
            )
            .ToList(),
        OnCompletionWindowDataPopulated = completionRequest.OnCompletionWindowDataPopulated
    };

    private void ShowCompletions(CompletionRequest request) => ShowCompletions(CreateShowCompletionOptions(request));
    private void ShowCompletions(ShowCompletionOptions options) => _completionWindow.ShowCompletions(options);

    private async Task<bool> ShowPathCompletions()
    {
        // Avoid unnecessary IO calls
        if (_completionWindow.IsOpen) return false;

        var currentLineText = _editorHelper.GetCurrentLineText();

        var result = await _vm._intellisenseClient.GetPathCompletions(currentLineText);
        if (result.IsEmpty()) return false;

        var opts = new CompletionRequest
        {
            Completions = result.Entries,
            Rewind = 0,
            OnCompletionWindowDataPopulated = completionWindow =>
            {
                // https://github.com/AvaloniaUI/AvaloniaEdit/blob/c8b0d481ec65eaebec2167165e79f62c363462b7/src/AvaloniaEdit/CodeCompletion/CompletionWindow.cs#L224-L257
                // https://github.com/AvaloniaUI/AvaloniaEdit/blob/c8b0d481ec65eaebec2167165e79f62c363462b7/src/AvaloniaEdit/CodeCompletion/CompletionList.cs#L336
                // Avalon(ia)Edit's CompletionWindow behavior is mostly independent of our code once the completion window is open.
                // Its internal filtering and selection algorithm is not a simple Contains or StartsWith. It calculates a quality score based on a prioritized list of string comparison strategies.
                // It is also transitively dependent on 2 variables with different lifecycles:
                // 1. The starting offset of the texteditor's caret when the CompletionWindow was first opened
                // 2. Its current offset of the texteditor's caret as the user types
                // Therefore, we need to be cognizant when implementing features related to the caret position.


                // This ensures consistent completion behavior when we lose focus and resume typing for paths.
                // Since path completion is unique in that path completion can start from a letter (e.g. "/myFol" => "/myFold"), we need to ensure that not only does the insertion "rewind" consistently,
                // the filtering algorithm also produces consistent results as we traverse through the lifecycles of both the starting offset and the current offset.
                // Typing "/myFold" should produce the same list of available results as typing "/myFol" (focus loss) + "d".
                // For every key stroke in "/myFolder", its result should be the same as doing the same, but pressing "ESC" before the next keystroke.
                // Since we already adjusted the cursor's starting offset, we do not need to add any additional "rewinding" in the ICompletionData we pass to the completion window.
                completionWindow.StartOffset = _textEditor.CaretOffset - result.Filter.Length;


                // when editor loses focus mid-path and user resumes typing, it won't require a second keypress to select the relevant result
                completionWindow.CompletionList.SelectItem(result.Filter);
                if (!completionWindow.CompletionList.CurrentList.Any())
                    completionWindow.Close();
            }
        };

        ShowCompletions(opts);


        return true;


    }

    public async Task HandleKeyDown(TextInputEventArgs e)
    {
        // don't return early here if it successfully closes, will break nested dir autocompletion i.e. /abc/d/...
        _completionWindow.CloseIfEmpty();

        if (await ShowPathCompletions())
            return;

        var options = e.Text switch
        {
            // only show completions if we are at the start of a line
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
            _ => CompletionRequest.Empty
        };

        ShowCompletions(options);

    }


    public void Dispose()
    {
        _textEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        _completionWindow.Close();
    }
}