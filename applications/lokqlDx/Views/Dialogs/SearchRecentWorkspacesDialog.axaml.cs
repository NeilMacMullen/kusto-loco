using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LokqlDx.ViewModels.Dialogs;

namespace LokqlDx.Views.Dialogs;

public partial class SearchRecentWorkspacesDialog : UserControl
{
    public SearchRecentWorkspacesDialog()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, RoutedEventArgs e) => FilterBox.Focus();

    private void FilterBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not SearchRecentWorkspacesViewModel vm)
            return;

        switch (e.Key)
        {
            case Key.Down:
                vm.SelectNext();
                e.Handled = true;
                break;
            case Key.Up:
                vm.SelectPrevious();
                e.Handled = true;
                break;
        }

        if (e.Handled && vm.SelectedItem is not null)
            WorkspaceList.ScrollIntoView(vm.SelectedItem);
    }
}
