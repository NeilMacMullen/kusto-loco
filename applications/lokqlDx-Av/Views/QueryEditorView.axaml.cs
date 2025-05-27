using System.Text;
using System.Xml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using lokqlDx;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public partial class QueryEditorView : UserControl, IDisposable
{
    private readonly EditorHelper _editorHelper;

    public QueryEditorView()
    {
        InitializeComponent();
        _editorHelper = new EditorHelper(TextEditor);
        TextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        //TextEditor.TextArea.SelectionChanged += TextArea_SelectionChanged;
        //HotKeyManager.SetHotKey(TextEditor, new(Key.Enter, KeyModifiers.Shift));
    }

    public void Dispose() => TextEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;

    //TextEditor.TextArea.SelectionChanged -= TextArea_SelectionChanged;
    private void TextArea_SelectionChanged(object? sender, EventArgs e)
    {
        if (DataContext is QueryEditorViewModel vm) vm.QueryText = GetTextAroundCursor();
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        if (DataContext is QueryEditorViewModel vm) vm.QueryText = GetTextAroundCursor();
    }

    /// <summary>
    ///     searches for lines around the cursor that contain text
    /// </summary>
    /// <remarks>
    ///     This allows us to easily run multi-line queries
    /// </remarks>
    private string GetTextAroundCursor()
    {
        return _editorHelper.GetTextAroundCursor();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        using var s = ResourceHelper.SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}
