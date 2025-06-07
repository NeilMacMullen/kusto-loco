using System.Diagnostics.CodeAnalysis;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using Intellisense;
using lokqlDx;
using LokqlDx.ViewModels;
using NotNullStrings;

namespace LokqlDx.Views;

public class CompletionManager : IDisposable
{
    private readonly TextEditor _textEditor;
    private readonly EditorHelper _editorHelper;
    private readonly QueryEditorViewModel _vm;
    private CompletionWindow? _completionWindow;

    public CompletionManager(TextEditor editor,
        EditorHelper editorHelper,
        QueryEditorViewModel vm
        )
    {
        _textEditor = editor;
        _editorHelper = editorHelper;
        _vm = vm;
        _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
    }

    [SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
    private async void Caret_PositionChanged(object? sender, EventArgs e)
    {
        try
        {
            await _vm._intellisenseClient.CancelRequestAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }



    private async Task<bool> ShowPathCompletions()
    {
        // Avoid unnecessary IO calls
        if (_completionWindow is not null) return false;


        var path = _vm.Parser.GetLastArgument(_editorHelper.GetCurrentLineText());

        if (path.IsBlank()) return false;




        var result = CompletionResult.Empty;
        try
        {
            // TODO: top level error handler or handle this properly with logger
            // TODO: discreetly notify user (status bar? notifications inbox?) to check connection status of saved connections
            // and user profile app was started with if hosts don't show shares
            result = await _vm._intellisenseClient.GetCompletionResultAsync(path);

        }
        catch (Exception e) when (e is IntellisenseException or OperationCanceledException)
        {
            // ignored
        }
        catch (Exception)
        {
            // ignored
        }

        if (!result.IsEmpty())
            ShowCompletions(
                result.Entries,
                string.Empty,
                result.Filter.Length,
                completionWindow =>
                {
                    // when editor loses focus mid-path and user resumes typing,
                    // it won't require a second keypress to select the relevant result
                    completionWindow.CompletionList.SelectItem(result.Filter);
                    if (!completionWindow.CompletionList.CurrentList.Any())
                        completionWindow.Close();
                });
        return true;


    }
    private void ShowCompletions(IReadOnlyList<IntellisenseEntry> completions, string prefix, int rewind,
        Action<CompletionWindow>? onCompletionWindowDataPopulated = null)
    {
        if (!completions.Any())
            return;

        _completionWindow = new CompletionWindow(_textEditor.TextArea)
        {
            CloseWhenCaretAtBeginning = true,
            MaxWidth = 200
        };
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
        foreach (var k in completions.OrderBy(k => k.Name))
            data.Add(new QueryEditorCompletionData(k, prefix, rewind));

        _completionWindow.Closed += delegate { _completionWindow = null; };
        onCompletionWindowDataPopulated?.Invoke(_completionWindow);
        _completionWindow?.Show();
    }
    public async Task HandleKeyDown(TextInputEventArgs e)
    {
        if (_completionWindow != null && !_completionWindow.CompletionList.CurrentList.Any())
        {
            _completionWindow.Close();
            // return;
        }

        if (await ShowPathCompletions())
            return;

        if (e.Text == ".")
        {
            //only show completions if we are at the start of a line
            var textToLeft = _editorHelper.TextToLeftOfCaret();
            if (textToLeft.TrimStart() == ".")
                ShowCompletions(
                    _vm.InternalCommands,
                    string.Empty, 0);
            return;
        }

        if (e.Text == "|")
        {
            ShowCompletions(

                _vm.KqlOperatorEntries,
                " ", 0);
            return;
        }

        if (e.Text == "@")
        {
            var blockText = _editorHelper.GetTextAroundCursor();
              var columns = _vm.SchemaIntellisenseProvider.GetColumns(blockText);
              ShowCompletions(columns, string.Empty, 1);
            return;
        }

        if (e.Text == "[")
        {
              var blockText = _editorHelper. GetTextAroundCursor();
               var tables = _vm.SchemaIntellisenseProvider.GetTables(blockText);
               ShowCompletions(tables, string.Empty, 1);
            return;
        }

        if (e.Text == "$")
        {
            ShowCompletions(_vm.SettingNames, string.Empty, 0);
            return;
        }

        if (e.Text == "?")
        {
            ShowCompletions(
                _vm.KqlFunctionEntries,
                string.Empty, 1);
        }
    }


    public void Dispose()
    {
        _textEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        _completionWindow?.Close();

    }
}
