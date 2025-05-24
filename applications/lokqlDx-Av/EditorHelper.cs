using AvaloniaEdit;
using AvaloniaEdit.Document;
using NotNullStrings;

namespace lokqlDx;

public class EditorHelper(TextEditor query)
{
    public TextEditor Query { get; set; } = query;

    public int GetCurrentLineNumber => LineAtCaret().LineNumber;

    public string GetCurrentLineText() => TextInLine(GetCurrentLineNumber);

    public string GetText(DocumentLine line)
    {
        return Query.Document.GetText(line.Offset, line.Length);
    }

    private bool LineNumberIsValid(int line)
    {
        return line >= 1 && line <= Query.Document.LineCount;
    }

    public string TextInLine(int line)
    {
        if (!LineNumberIsValid(line))
            return string.Empty;
        return GetText(Query.Document.GetLineByNumber(line));
    }

    public bool LineIsTopOfBlock(int line)
    {
        return TextInLine(line - 1).IsBlank() & TextInLine(line).IsNotBlank();
    }

    public DocumentLine LineAtCaret()
    {
        return Query.Document.GetLineByOffset(Query.CaretOffset);
    }

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
}
