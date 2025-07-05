using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Utils;
using lokqlDxComponents.Models;

namespace lokqlDxComponents.Views.Dialogs;

public class CompletionWindowWrapper(TextArea textArea)
{
    private CompletionWindow? _completionWindow;

    public bool IsOpen => _completionWindow?.IsOpen ?? false;

    public IReadOnlyList<ICompletionData> GetCurrentCompletionListEntries() => _completionWindow?.CompletionList.CurrentList ?? [];

    public void CloseIfEmpty()
    {
        if (_completionWindow != null && !GetCurrentCompletionListEntries().Any())
        {
            _completionWindow.Close();
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
        var onCompletionWindowDataPopulated = options.OnCompletionWindowDataPopulated;
        if (!completions.Any())
            return;

        _completionWindow = new CompletionWindow(textArea)
        {
            CloseWhenCaretAtBeginning = true,
            MaxWidth = 200
        };
        IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
        data.AddRange(options.Completions);

        _completionWindow.Closed += delegate { _completionWindow = null; };
        onCompletionWindowDataPopulated.Invoke(_completionWindow);
        _completionWindow?.Show();
    }

}
