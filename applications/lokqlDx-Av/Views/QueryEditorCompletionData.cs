using Avalonia.Media;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Intellisense;
using NotNullStrings;

namespace LokqlDx.Views;

/// Implements AvalonEdit ICompletionData interface to provide the entries in the
/// completion drop down.
public class QueryEditorCompletionData(IntellisenseEntry entry, string prefix, int rewind) : ICompletionData
{
    public string Text => entry.Name;

    // Use this property if you want to show a fancy UIElement in the list.
    public object Content => Text;

    public object Description
        => entry.Syntax.IsBlank()
            ? entry.Description
            : $@"{entry.Description}
Usage: {entry.Syntax}";


    public double Priority => 1.0;

    public void Complete(TextArea textArea, ISegment completionSegment,
        EventArgs insertionRequestEventArgs)
    {
        var seg = new TextSegment
        {
            StartOffset = completionSegment.Offset - rewind,
            Length = completionSegment.Length + rewind
        };
        textArea.Document.Replace(seg, prefix + Text);
    }

    public IImage? Image { get; } = null;
}
