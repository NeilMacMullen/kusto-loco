using System.Xml;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace LokqlDx.Views.Dialogs;

public partial class WorkspacePreferencesView : UserControl
{
    public WorkspacePreferencesView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        using var s = ResourceHelper.SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}
