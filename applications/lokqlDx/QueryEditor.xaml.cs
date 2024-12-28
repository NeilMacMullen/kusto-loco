using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using NotNullStrings;

namespace lokqlDx;

/// <summary>
///     Simple text editor for running queries
/// </summary>
/// <remarks>
///     The key thing this provides is an event based on SHIFT-ENTER that
///     selects text around the cursor and sends it to the RunEvent
/// </remarks>
public partial class QueryEditor : UserControl
{
    private bool _isBusy;

    public QueryEditor()
    {
        InitializeComponent();
    }

    #region public interface

    public event EventHandler<QueryEditorRunEventArgs>? RunEvent;

    #endregion


    /// <summary>
    ///     searches for lines around the cursor that contain text
    /// </summary>
    /// <remarks>
    ///     This allows us to easily run multi-line queries
    /// </remarks>
    private string GetTextAroundCursor()
    {
        if (Query.SelectionLength > 0) return Query.SelectedText.Trim();

        var i = Query
            .Document.GetLineByOffset(Query.CaretOffset).LineNumber;

        string GetLineForDocumentLine(DocumentLine line)
        {
            return Query.Document.GetText(line.Offset, line.Length);
        }

        string GetLineForLine(int line)
        {
            return GetLineForDocumentLine(Query.Document.GetLineByNumber(line));
        }

        var sb = new StringBuilder();

        while (i > 1 && GetLineForLine(i - 1).Trim().Length > 0)
            i--;
        while (i <= Query.LineCount && GetLineForLine(i).Trim().Length > 0)
        {
            sb.AppendLine(GetLineForLine(i));
            i++;
        }

        return sb.ToString().Trim();
    }

    public void SetFontSize(double newSize)
    {
        Query.FontSize = newSize;
    }

    public void SetText(string text)
    {
        Query.Text = text;
    }

    public void SetFont(string font)
    {
        Query.FontFamily = new FontFamily(font);
    }

    public string GetText()
    {
        return Query.Text;
    }

    public void SetBusy(bool isBusy)
    {
        _isBusy = isBusy;
        BusyStatus.Content = isBusy ? "Busy" : "Ready";
    }

    private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
    {
        if (_isBusy)
        {
            e.Handled = false;
            return;
        }

        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Handled = true;
    }

    private void InsertAtCursor(string text)
    {
        Query.Document.Insert(Query.CaretOffset, text);
      
    }

    private void TextBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            var newString = files.Select(f => $".{VerbFromExtension(f)} \"{f}\"").JoinAsLines();
            InsertAtCursor(newString);
        }

        e.Handled = true;

        string VerbFromExtension(string f)
        {
            return f.EndsWith(".csl") ? "run" : "load";
        }
    }

    private void Query_OnPreviewDragEnter(object sender, DragEventArgs drgevent)
    {
        drgevent.Handled = true;


        // Check that the data being dragged is a file
        if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Get an array with the filenames of the files being dragged
            var files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

            if (files.Length > 0)
                drgevent.Effects = DragDropEffects.Move;
            else
                drgevent.Effects = DragDropEffects.None;
        }
        else
        {
            drgevent.Effects = DragDropEffects.None;
        }
    }

    public void SetWordWrap(bool wordWrap)
    {
        Query.WordWrap = wordWrap;
    }

    public void ShowLineNumbers(bool show)
    {
        Query.ShowLineNumbers = show;
    }

    private void InternalEditor_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            if (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                e.Handled = true;
                var query = GetTextAroundCursor();
                if (query.Length > 0) RunEvent?.Invoke(this, new QueryEditorRunEventArgs(query));
            }
    }

    private void QueryEditor_OnLoaded(object sender, RoutedEventArgs e)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var s = assembly.GetManifestResourceStream("lokqlDx.SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s!);
        Query.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}

public class QueryEditorRunEventArgs(string query) : EventArgs
{
    public string Query { get; private set; } = query;
}
