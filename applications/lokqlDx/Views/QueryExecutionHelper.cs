using Avalonia.Input;
using LokqlDx.ViewModels;
using lokqlDxComponents;

namespace LokqlDx.Views;

public static class QueryExecutionHelper
{
    public static async Task HandleKeyCombo(KeyEventArgs e,
        EditorHelper _editorHelper,
        QueryEditorViewModel vm)
    {
        var isEnter = e.Key is Key.Enter;
        var shiftDown = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var ctrlDown = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var altDown = e.KeyModifiers.HasFlag(KeyModifiers.Alt);

        if (isEnter && shiftDown)
        {
            e.Handled = true;
            var query = ctrlDown
                ? _editorHelper.GetFullText()
                : _editorHelper.GetTextAroundCursor();

            await vm.RunQueryCommand.ExecuteAsync(query);
        }

        if (altDown && shiftDown && e.Key is Key.F)
        {
            e.Handled = true;
            var text = _editorHelper.GetFullText();
            await vm.RunFormatCommand.ExecuteAsync(text);
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
