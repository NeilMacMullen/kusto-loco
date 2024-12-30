using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using KustoLoco.Core.Settings;
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
    private readonly EditorHelper editorHelper;
    private IntellisenseEntry[] _columnNames = [];
    private IntellisenseEntry[] _internalCommands = [];
    private bool _isBusy;
    private IntellisenseEntry[] _settingNames = [];
    private IntellisenseEntry[] _tableNames = [];
    private CompletionWindow? completionWindow;
    private IntellisenseEntry[] KqlFunctionEntries = [];

    public IntellisenseEntry[] KqlOperatorEntries = [];

    public QueryEditor()
    {
        InitializeComponent();
        Query.TextArea.TextEntering += textEditor_TextArea_TextEntering;
        Query.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        editorHelper = new EditorHelper(Query);
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

        var i = editorHelper.LineAtCaret().LineNumber;

        var sb = new StringBuilder();

        while (i > 1 && editorHelper.TextInLine(i - 1).Trim().Length > 0)
            i--;
        while (i <= Query.LineCount && editorHelper.TextInLine(i).Trim().Length > 0)
        {
            sb.AppendLine(editorHelper.TextInLine(i));
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

    public string GetText() => Query.Text;

    public void SetBusy(bool isBusy)
    {
        _isBusy = isBusy;
        BusyStatus.Content = isBusy
                                 ? "Busy"
                                 : "Ready";
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
            => f.EndsWith(".csl")
                   ? "run"
                   : "load";
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

        using var functions = assembly.GetManifestResourceStream("lokqlDx.IntellisenseFunctions.json");
        KqlFunctionEntries = JsonSerializer.Deserialize<IntellisenseEntry[]>(functions!)!;
        using var ops = assembly.GetManifestResourceStream("lokqlDx.IntellisenseOperators.json");
        KqlOperatorEntries = JsonSerializer.Deserialize<IntellisenseEntry[]>(ops!)!;
    }

    private void ShowCompletions(IEnumerable<IntellisenseEntry> completions, string prefix, int rewind)
    {
        completionWindow = new CompletionWindow(Query.TextArea);
        completionWindow.CloseWhenCaretAtBeginning = true;
        IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
        foreach (var k in completions.OrderBy(k => k.Name))
            data.Add(new MyCompletionData(k, prefix, rewind));
        completionWindow.Show();
        completionWindow.Closed += delegate { completionWindow = null; };
    }

    private void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
    {
        if (completionWindow != null && !completionWindow.CompletionList.ListBox.HasItems)
        {
            completionWindow.Close();
            return;
        }

        if (e.Text == ".")
        {
            //only show completions if we are at the start of a line
            var textToLeft = editorHelper.TextToLeftOfCaret();
            if (textToLeft.TrimStart() == ".")
            {
                ShowCompletions(_internalCommands, string.Empty, 0);
            }


            return;
        }

        if (e.Text == "|")
        {
            ShowCompletions(KqlOperatorEntries, " ", 0);
            return;
        }

        if (e.Text == "@")
        {
            ShowCompletions(_columnNames, string.Empty, 1);
            return;
        }

        if (e.Text == "[")
        {
            ShowCompletions(_tableNames, string.Empty, 1);
            return;
        }

        if (e.Text == "$")
        {
            ShowCompletions(_settingNames, string.Empty, 0);
            return;
        }

        if (e.Text == "?")
        {
            ShowCompletions(KqlFunctionEntries, string.Empty, 1);
        }
    }

    public void AddInternalCommands(IntellisenseEntry[] verbs)
    {
        _internalCommands = verbs;
    }

    private void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
    {
        if (e.Text.Length > 0 && completionWindow != null)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                // Whenever a non-letter is typed while the completion window is open,
                // insert the currently selected element.
                completionWindow.CompletionList.RequestInsertion(e);

                // Do not set e.Handled=true.
                // We still want to insert the character that was typed.
            }
        }
    }

    public void SetColumnNames(string[] getTablesAndSchemas)
    {
        _columnNames = getTablesAndSchemas.Select(t => new IntellisenseEntry(t, "Column", t))
                                          .ToArray();
    }

    public void SetTableNames(string[] getTablesAndSchemas)
    {
        _tableNames = getTablesAndSchemas.Select(t => new IntellisenseEntry(t, "Table", t))
                                         .ToArray();
    }

    public void AddSettingsForIntellisense(KustoSettingsProvider settings)
    {
        _settingNames = settings.Enumerate()
                                .Select(s => new IntellisenseEntry(s.Name, s.Value, string.Empty))
                                .ToArray();
    }
}

public class QueryEditorRunEventArgs(string query) : EventArgs
{
    public string Query { get; private set; } = query;
}

/// Implements AvalonEdit ICompletionData interface to provide the entries in the
/// completion drop down.
public class MyCompletionData(IntellisenseEntry entry, string prefix, int rewind) : ICompletionData
{
    public ImageSource? Image => null;

    public string Text => entry.Name;

    // Use this property if you want to show a fancy UIElement in the list.
    public object? Content => Text;

    public object? Description
        => $@"{entry.Description}
Usage: {entry.Syntax}";


    public double Priority => 1.0;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        var seg = new TextSegment();
        seg.StartOffset = completionSegment.Offset - rewind;
        seg.Length = completionSegment.Length + rewind;
        textArea.Document.Replace(seg, prefix + Text);
    }
}

public class EditorHelper
{
    public EditorHelper(TextEditor query) => Query = query;

    public TextEditor Query { get; set; }


    public string GetText(DocumentLine line) => Query.Document.GetText(line.Offset, line.Length);

    public string TextInLine(int line) => GetText(Query.Document.GetLineByNumber(line));

    public DocumentLine LineAtCaret() => Query.Document.GetLineByOffset(Query.CaretOffset);

    public string TextToLeftOfCaret()
    {
        var line = LineAtCaret();
        return Query.Document.GetText(line.Offset, Query.CaretOffset - line.Offset);
    }
}

public readonly record struct IntellisenseEntry(string Name, string Description, string Syntax);
