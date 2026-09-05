using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LokqlDx.Views.Dialogs;

public partial class SearchRecentWorkspacesDialog : UserControl
{
    public SearchRecentWorkspacesDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) => FilterBox.Focus();
}
