using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.CodeCompletion;
using Intellisense;
using lokqlDx;
using LokqlDx.ViewModels;
using NotNullStrings;

namespace LokqlDx.Views;

public class CompletionManager
{
    private readonly TextEditor TextEditor;
    private readonly EditorHelper _editorHelper;

    public CompletionManager(AvaloniaEdit.TextEditor editor,
        EditorHelper editorHelper)
    {
        TextEditor = editor;
        _editorHelper = editorHelper;
    }
    private CompletionWindow? _completionWindow;
    private async Task<bool> ShowPathCompletions(QueryEditorViewModel vm)
    {
        await Task.CompletedTask;
        // Avoid unnecessary IO calls
        if (_completionWindow is not null) return false;

        var path = vm.Parser.GetLastArgument(_editorHelper.GetCurrentLineText());

        if (path.IsBlank()) return false;

#if true
        return false;
#else
        var result = await vm._intellisenseClient.GetCompletionResultAsync(path);
        if (result.IsEmpty())


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
#endif

    }
    private void ShowCompletions(IReadOnlyList<IntellisenseEntry> completions, string prefix, int rewind,
        Action<CompletionWindow>? onCompletionWindowDataPopulated = null)
    {
        if (!completions.Any())
            return;

        _completionWindow = new CompletionWindow(TextEditor.TextArea)
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
    public async Task HandleKeyDown(TextInputEventArgs e,QueryEditorViewModel vm)
    {
        if (_completionWindow != null && !_completionWindow.CompletionList.CurrentList.Any())
        {
            _completionWindow.Close();
            return;
        }

        if (await ShowPathCompletions(vm))
            return;

        if (e.Text == ".")
        {
            //only show completions if we are at the start of a line
            var textToLeft = _editorHelper.TextToLeftOfCaret();
            if (textToLeft.TrimStart() == ".")
                ShowCompletions(
                    vm.InternalCommands,
                    string.Empty, 0);
            return;
        }

        if (e.Text == "|")
        {
            ShowCompletions(
              
                vm.KqlOperatorEntries,
                " ", 0);
            return;
        }

        if (e.Text == "@")
        {
            var blockText = _editorHelper.GetTextAroundCursor();
              var columns = vm.SchemaIntellisenseProvider.GetColumns(blockText);
              ShowCompletions(columns, string.Empty, 1);
            return;
        }

        if (e.Text == "[")
        {
              var blockText = _editorHelper. GetTextAroundCursor();
               var tables = vm.SchemaIntellisenseProvider.GetTables(blockText);
               ShowCompletions(tables, string.Empty, 1);
            return;
        }

        if (e.Text == "$")
        {
            ShowCompletions(vm.SettingNames, string.Empty, 0);
            return;
        }

        if (e.Text == "?")
        {
            ShowCompletions(
                vm.KqlFunctionEntries,
                string.Empty, 1);
            return;
        }
    }


}
