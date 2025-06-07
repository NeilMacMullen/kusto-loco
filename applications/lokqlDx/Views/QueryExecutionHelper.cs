using Avalonia.Input;
using lokqlDx;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public static class QueryExecutionHelper
{
    public static async Task  HandleKeyCombo(KeyEventArgs e,
        EditorHelper _editorHelper,
        QueryEditorViewModel vm)
    {
        var isEnter = e.Key is Key.Enter;
        var shiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var ctrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);

        if (isEnter && shiftDown)
        {
            e.Handled = true;
            var query = ctrlDown
                ? _editorHelper.GetFullText()
                : _editorHelper.GetTextAroundCursor();

            await vm.RunQueryCommand.ExecuteAsync(query);
        }

        if (e.Key == Key.Down && ctrlDown)
        {
            e.Handled = true;
            _editorHelper.ScrollDownToComment();
        }

        if (e.Key == Key.Up && ctrlDown)
        {
            e.Handled = true;
            _editorHelper.ScrollUpToComment();
        }
    }
}
