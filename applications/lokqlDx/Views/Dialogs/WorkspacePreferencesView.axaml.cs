using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LokqlDx.Views.Dialogs;

public partial class WorkspacePreferencesView : UserControl
{
    public WorkspacePreferencesView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) =>
        HighlightHelper.ApplySyntaxHighlighting(TextEditor);
}
