using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit;

namespace LokqlDx.Views.Dialogs;

public partial class WorkspacePreferencesView : UserControl
{
    public WorkspacePreferencesView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e)
    {
        var textEditor = this.FindControl<TextEditor>("TextEditor");
        if (textEditor != null)
            HighlightHelper.ApplySyntaxHighlighting(textEditor);
    }
}
