using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using lokqlDxComponents.Models;

namespace lokqlDxComponents.Views.Dialogs;

public class CompletionWindowWrapper(TextArea textArea)
{
    private CompletionWindow? _completionWindow;

    public bool IsOpen => _completionWindow?.IsOpen ?? false;

    public IReadOnlyList<ICompletionData> GetCurrentCompletionListEntries()
    {
        return _completionWindow?.CompletionList.CurrentList ?? [];
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
