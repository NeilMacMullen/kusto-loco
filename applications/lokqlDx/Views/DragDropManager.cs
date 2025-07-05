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
        drgevent.DragEffects = drgevent.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    public static void DragEnter(DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    public static void Drop(DragEventArgs e, EditorHelper editorHelper)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var newString = e.Data.GetFiles()!
                .Select(s => s.TryGetLocalPath().NullToEmpty())
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
