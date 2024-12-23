using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using NotNullStrings;

namespace lokqlDx;

/// <summary>
///     Simple text editor for running queries
/// </summary>
/// <remarks>
///     The key thing this provides is an event based on SHIFT-ENTER that
///     selects text around the cursor and sends it to the RunEvent
///     In future it would be nice to replace this with a more capable editor
///     that supports syntax highlighting and other features
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

    private void Query_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            if (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                e.Handled = true;
                var query = GetTextAroundCursor();
                if (query.Length > 0) RunEvent?.Invoke(this, new QueryEditorRunEventArgs(query));
            }
    }

    /// <summary>
    ///     searches for lines around the cursor that contain text
    /// </summary>
    /// <remarks>
    ///     This allows us to easily run multi-line queries
    /// </remarks>
    public string GetTextAroundCursor()
    {
        if (Query.SelectionLength > 0) return Query.SelectedText.Trim();

        var i = Query.GetLineIndexFromCharacterIndex(Query.CaretIndex);
        var sb = new StringBuilder();
        while (i >= 1 && Query.GetLineText(i - 1).Trim().Length > 0)
            i--;
        while (i < Query.LineCount && Query.GetLineText(i).Trim().Length > 0)
        {
            sb.Append(Query.GetLineText(i));
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

    private void InsertAtCursor(string newString)
    {
        Query.Text = Query.Text.Insert(Query.CaretIndex, newString);
        Query.CaretIndex += newString.Length;
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
        string VerbFromExtension(string f) => f.EndsWith(".csl") ? "run" : "load";
    }

    private void Query_OnPreviewDragEnter(object sender, DragEventArgs drgevent)
    {
        drgevent.Handled = true;


        // Check that the data being dragged is a file
        if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
        {
            // Get an array with the filenames of the files being dragged
            var files = (string[])drgevent.Data.GetData(DataFormats.FileDrop);

            if (files.Length >0)
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
        Query.TextWrapping = wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
    }
}

public class QueryEditorRunEventArgs(string query) : EventArgs
{
    public string Query { get; private set; } = query;
}
