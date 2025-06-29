using System.Text;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using NotNullStrings;

namespace lokqlDxComponents;

public class EditorHelper(TextEditor query) : IEditorCursorViewModel
{
    public TextEditor Query { get; set; } = query;

    public int GetCurrentLineNumber => LineAtCaret().LineNumber;

    public string GetCurrentLineText() => TextInLine(GetCurrentLineNumber);

    public string GetText(DocumentLine line) => Query.Document.GetText(line.Offset, line.Length);

    private bool LineNumberIsValid(int line) => line >= 1 && line <= Query.Document.LineCount;

    public string TextInLine(int line) =>
        !LineNumberIsValid(line)
            ? string.Empty
            : GetText(Query.Document.GetLineByNumber(line));

    public bool LineIsTopOfBlock(int line) => TextInLine(line - 1).IsBlank() & TextInLine(line).IsNotBlank();

    public DocumentLine LineAtCaret() => Query.Document.GetLineByOffset(Query.CaretOffset);

    public string TextToLeftOfCaret()
    {
        var line = LineAtCaret();
        return Query.Document.GetText(line.Offset, Query.CaretOffset - line.Offset);
    }

    public void ScrollToLine(int line)
    {
        if (!LineNumberIsValid(line))
            return;

        Query.CaretOffset =
            Query.Document.GetLineByNumber(line).Offset;
        Query.ScrollToLine(line);
    }

    public string GetFullText() => Query.Text;

    public string GetTextAroundCursor()
    {
        if (Query.SelectionLength > 0) return Query.SelectedText.Trim();

        var i = LineAtCaret().LineNumber;

        var sb = new StringBuilder();

        while (i > 1 && TextInLine(i - 1).Trim().Length > 0)
            i--;
        while (i <= Query.LineCount && TextInLine(i).Trim().Length > 0)
        {
            sb.AppendLine(TextInLine(i));
            i++;
        }

        return sb.ToString().Trim();
    }

    public void ScrollDownToComment()
    {
        var i = GetCurrentLineNumber + 1;

        while (!LineIsTopOfBlock(i) && i <= Query.LineCount)
            i++;
        ScrollToLine(i);
    }

    public void ScrollUpToComment()
    {
        var i = GetCurrentLineNumber - 1;

        while (!LineIsTopOfBlock(i) && i >= 1)
            i--;
        ScrollToLine(i);
    }

    public void InsertAtCursor(string text) => Query.Document.Insert(Query.CaretOffset, text);
}
