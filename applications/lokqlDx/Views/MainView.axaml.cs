using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using DependencyPropertyGenerator;
using LokqlDx.Models;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

[DependencyProperty<ColumnDefinitions>(
    "ColumnDefinitions",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<RowDefinitions>(
    "RowDefinitions",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class MainView : UserControl
{
    private MenuItem? _recentlyUsedMenuItem;
    private Menu? _mainMenu;
    private MainViewModel? _viewModel;

    public MainView()
    {
        InitializeComponent();
        _recentlyUsedMenuItem = this.FindControl<MenuItem>("RecentlyUsed");
        _mainMenu = this.FindControl<Menu>("MainMenu");
        DataContextChanged += OnDataContextChanged;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RecentWorkspaces.CollectionChanged -= OnRecentWorkspacesChanged;

        _viewModel = DataContext as MainViewModel;

        if (_viewModel is not null)
        {
            _viewModel.RecentWorkspaces.CollectionChanged += OnRecentWorkspacesChanged;
            RebuildRecentWorkspacesMenu();
        }
    }

    private void OnRecentWorkspacesChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        // Defer the rebuild to a later dispatcher pass. Rebuilding the Items collection
        // synchronously (e.g. while a recent-workspace MenuItem's Click handler is still
        // executing, as part of loading the selected workspace) destroys the MenuItem that
        // is currently being clicked and corrupts Avalonia's menu/popup closing pipeline,
        // leaving the File menu and submenu overlay visually open. Posting at Background
        // priority lets the click/close interaction finish first.
        Dispatcher.UIThread.Post(RebuildRecentWorkspacesMenu, DispatcherPriority.Background);

    private void RebuildRecentWorkspacesMenu()
    {
        if (_recentlyUsedMenuItem is null || _viewModel is null)
            return;

        _recentlyUsedMenuItem.Items.Clear();
        foreach (var recentWorkspace in _viewModel.RecentWorkspaces)
        {
            var item = new MenuItem
            {
                Header = recentWorkspace.Name,
                Command = _viewModel.OpenRecentWorkspaceCommand,
                CommandParameter = recentWorkspace
            };
            //Avalonia (as of 12.1.5) does not reliably close the menu chain when a
            //dynamically-added MenuItem (one that is inserted into an Items collection
            //after the menu's visual tree has already been built) is clicked. Statically
            //declared MenuItems close correctly, but these recent-workspace entries are
            //rebuilt at runtime, so we explicitly close the menu ourselves as a workaround.
            item.Click += OnRecentWorkspaceItemClick;
            _recentlyUsedMenuItem.Items.Add(item);
        }
    }

    private void OnRecentWorkspaceItemClick(object? sender, RoutedEventArgs e)
    {
        CloseMenu();
    }

    private void CloseMenu()
    {
        if (_recentlyUsedMenuItem is not null)
            _recentlyUsedMenuItem.IsSubMenuOpen = false;

        if (_mainMenu is null)
            return;

        foreach (var item in _mainMenu.Items)
            if (item is MenuItem menuItem)
                menuItem.IsSubMenuOpen = false;

        _mainMenu.Close();
    }
}

