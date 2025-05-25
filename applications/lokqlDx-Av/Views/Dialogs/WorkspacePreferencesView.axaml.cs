using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit;
using System.Xml;

namespace LokqlDx.Views.Dialogs;

public partial class WorkspacePreferencesView : UserControl
{
    public WorkspacePreferencesView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        using var s = ResourceHelper.SafeGetResourceStream("SyntaxHighlighting.xml");
        using var reader = new XmlTextReader(s);
        TextEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
    }
}
