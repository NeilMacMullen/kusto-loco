namespace lokqlDxComponents.ViewModels;

public interface IEditorCursorViewModel
{
    string GetCurrentLineText();
    string TextToLeftOfCaret();
    string GetFullText();
    string GetTextAroundCursor();
    void InsertAtCursor(string text);
}
