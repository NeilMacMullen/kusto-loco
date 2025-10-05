using Avalonia.Input;
using Avalonia.Platform.Storage;
using lokqlDxComponents;
using NotNullStrings;

namespace LokqlDx.Views;

public static class DragDropManager
{
    public static void DragOver(DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.DataTransfer.Contains(DataFormat.File)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    public static void DragEnter(DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.DataTransfer.Contains(DataFormat.File)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    public static void Drop(DragEventArgs e, EditorHelper editorHelper)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {|
            var items = e.DataTransfer.TryGetFiles() ?? [];

            var newString = items
                .Select(s => s.TryGetLocalPath().NullToEmpty()!)
                .Where(s => s.IsNotBlank())
                .Select(f => $".{VerbFromExtension(f)} \"{f}\"")
                .JoinAsLines();
            editorHelper.InsertAtCursor(newString);
        }

        e.Handled = true;
        return;

        string VerbFromExtension(string f)
        {
            return f.EndsWith(".csl")
                ? "run"
                : "load";
        }
    }
}
