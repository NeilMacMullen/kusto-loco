using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.LogicalTree;
using AvaloniaEdit.Editing;

namespace lokqlDxComponentsTests;

public static class TextAreaExtensions
{
    public static async Task Type(this TextArea textArea, string text)
    {
        textArea.Focus();
        foreach (var c in text.Select(x => x.ToString()))
        {
            await Task.Delay(1);
            textArea.PerformTextInput(c);
        }
    }

    public static async Task Erase(this TextArea textArea, int count = 1)
    {
        textArea.Focus();
        var window = textArea.FindLogicalAncestorOfType<Window>()!;
        for (int i = 0; i < count; i++)
        {
            await Task.Delay(1);
            window.KeyPressQwerty(PhysicalKey.Backspace, RawInputModifiers.None);
        }
    }

    public static async Task Press(this TextArea textArea, PhysicalKey key)
    {
        textArea.Focus();
        var window = textArea.FindLogicalAncestorOfType<Window>()!;
        window.KeyPressQwerty(key, RawInputModifiers.None);
        await Task.Delay(1);
    }
}
