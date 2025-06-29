using AvaloniaEdit;
using AvaloniaEdit.Document;

namespace lokqlDxComponents.ViewModels;

public interface IEditorCursorViewModel
{
    TextEditor Query { get; set; }
    int GetCurrentLineNumber { get; }
    string GetCurrentLineText();
    string GetText(DocumentLine line);
    string TextInLine(int line);
    bool LineIsTopOfBlock(int line);
    DocumentLine LineAtCaret();
    string TextToLeftOfCaret();
    void ScrollToLine(int line);
    string GetFullText();
    string GetTextAroundCursor();
    void ScrollDownToComment();
    void ScrollUpToComment();
    void InsertAtCursor(string text);
}
