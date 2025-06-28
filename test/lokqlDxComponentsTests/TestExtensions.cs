using AvaloniaEdit.Editing;

namespace lokqlDxComponentsTests;

public static class TestExtensions
{
    public static void Type(this TextArea textArea, string text)
    {
        foreach (var c in text.Select(x => x.ToString()))
        {
            textArea.PerformTextInput(c);
        }
    }
}
