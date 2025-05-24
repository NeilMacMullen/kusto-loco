using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Highlighting;
using lokqlDx;
using LokqlDx.ViewModels;
using System.ComponentModel;
using System.Text;
using System.Reflection;
using System.Xml;

namespace LokqlDx.Views;

public partial class QueryEditorView : UserControl, IDisposable
{
    private EditorHelper _editorHelper;

    public QueryEditorView()
    {
        InitializeComponent();
        _editorHelper = new EditorHelper(TextEditor);
        TextEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
        //TextEditor.TextArea.SelectionChanged += TextArea_SelectionChanged;
        //HotKeyManager.SetHotKey(TextEditor, new(Key.Enter, KeyModifiers.Shift));
    }

    private void TextArea_SelectionChanged(object? sender, EventArgs e)
    {
        if (DataContext is QueryEditorViewModel vm)
        {
            vm.QueryText = GetTextAroundCursor();
        }
    }

    private void Caret_PositionChanged(object? sender, EventArgs e)
    {
        if (DataContext is QueryEditorViewModel vm)
        {
            vm.QueryText = GetTextAroundCursor();
        }
    }

    /// <summary>
    ///     searches for lines around the cursor that contain text
    /// </summary>
    /// <remarks>
    ///     This allows us to easily run multi-line queries
    /// </remarks>
    private string GetTextAroundCursor()
    {
        //if (Query.SelectionLength > 0) return Query.SelectedText.Trim();

        if (_editorHelper is null)
            return "";

        var i = _editorHelper.LineAtCaret().LineNumber;

        var sb = new StringBuilder();

        while (i > 1 && _editorHelper.TextInLine(i - 1).Trim().Length > 0)
            i--;
        while (i <= TextEditor.LineCount && _editorHelper.TextInLine(i).Trim().Length > 0)
        {
            sb.AppendLine(_editorHelper.TextInLine(i));
            i++;
        }

        return sb.ToString().Trim();
    }

    public void Dispose()
    {
        TextEditor.TextArea.Caret.PositionChanged -= Caret_PositionChanged;
        //TextEditor.TextArea.SelectionChanged -= TextArea_SelectionChanged;
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        using var s = SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }

    /// <summary>
    ///     Gets a resource name independent of namespace
    /// </summary>
    /// <remarks>
    ///     For some reason dotnet publish decides to lower-case the
    ///     namespace in the resource name. In any case, we really don't want to trust
    ///     that the namespace won't change so do a match against the filename
    /// </remarks>
    private Stream SafeGetResourceStream(string substring)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var availableResources = assembly.GetManifestResourceNames();
        var wanted =
            availableResources.Single(name => name.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        return assembly.GetManifestResourceStream(wanted)!;
    }
}
