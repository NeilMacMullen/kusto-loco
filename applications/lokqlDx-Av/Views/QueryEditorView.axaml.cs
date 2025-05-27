using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using lokqlDx;
using LokqlDx.ViewModels;
using NotNullStrings;
using System.Xml;
using Avalonia.Platform.Storage;

#pragma warning disable VSTHRD100

namespace LokqlDx.Views;

public partial class QueryEditorView : UserControl
{
    private readonly EditorHelper _editorHelper;

    public QueryEditorView()
    {
        InitializeComponent();
        _editorHelper = new EditorHelper(TextEditor);
    }

    private QueryEditorViewModel GetVm() =>
        (DataContext as QueryEditorViewModel)!;


    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        using var s = ResourceHelper.SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        //Ensure we can intercept ENTER key
        TextEditor.AddHandler(KeyDownEvent, InternalEditor_OnKeyDown, RoutingStrategies.Tunnel);
        TextEditor.AddHandler(DragDrop.DragEnterEvent, DragEnter);
        TextEditor.AddHandler(DragDrop.DragOverEvent, DragOver);

        TextEditor.AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragOver(object? sender, DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    private void DragEnter(object? sender, DragEventArgs drgevent)
    {
        drgevent.Handled = true;
        // Check that the data being dragged is a file
        drgevent.DragEffects = drgevent.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Link
            : DragDropEffects.None;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var newString = e.Data.GetFiles()!
                .Select(s => s.TryGetLocalPath().NullToEmpty())
                .Where(s => s.IsNotBlank())
                .Select(f => $".{VerbFromExtension(f)} \"{f}\"")
                .JoinAsLines();
                 _editorHelper.InsertAtCursor(newString);
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
    

    private async void InternalEditor_OnKeyDown(object? sender, KeyEventArgs e)
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

            await GetVm().RunQueryCommand.ExecuteAsync(query);
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
