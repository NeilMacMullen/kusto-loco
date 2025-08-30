using System.Xml.Linq;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public static class HighlightHelper
{
    public static void ApplySyntaxHighlighting(TextEditor editor)
    {
        var currentTheme = ApplicationHelper.GetTheme();
        ApplySyntaxHighlighting(editor, currentTheme);
    }

    public static void ApplySyntaxHighlighting(TextEditor editor, string theme)
    {
#if ! PREVIEWER
        var file = "VSDark";
        if (theme.Equals("light", StringComparison.InvariantCultureIgnoreCase))
            file = "VSLight";
        // Load VSDark.xml as XDocument
        using var colorStream = ResourceHelper.SafeGetResourceStream($"{file}.xml");
        var colorDoc = XDocument.Load(colorStream);

        // Load SyntaxHighlighting.xml as XDocument
        using var syntaxStream = ResourceHelper.SafeGetResourceStream("SyntaxHighlighting.xml");
        var syntaxDoc = XDocument.Load(syntaxStream);

        // Insert <Color> elements from VSDark.xml into <SyntaxDefinition>
        var syntaxDef = syntaxDoc.Root;
        var colorElements = colorDoc.Root!.Elements().Where(e => e.Name.LocalName == "Color");
        // Insert at the top, before any RuleSet
        var firstRuleSet = syntaxDef!.Elements().FirstOrDefault(e => e.Name.LocalName == "RuleSet");
        if (firstRuleSet == null)
            throw new InvalidOperationException("Error in syntaxhighlighting");
        foreach (var color in colorElements) firstRuleSet.AddBeforeSelf(new XElement(color));

        // Load the combined XML into the highlighter
        using var combinedReader = syntaxDoc.CreateReader();
        var def = HighlightingLoader.Load(combinedReader, HighlightingManager.Instance);
        editor.SyntaxHighlighting = def;
#endif
    }
}
