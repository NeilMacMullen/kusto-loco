namespace LokqlDx.ViewModels;

public class RenamableText
{
    public string InitialText = string.Empty;
    public string NewText;

    public RenamableText(string initialText)
    {
        InitialText = initialText;
        NewText = initialText;
    }
}
