using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LokqlDx.Views.Dialogs;

public partial class EditWorkspaceDescriptionDialog : UserControl
{
    public EditWorkspaceDescriptionDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) => DescriptionBox.Focus();
}
