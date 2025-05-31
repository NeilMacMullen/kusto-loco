using Avalonia.Controls;
using Avalonia.Interactivity;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

#pragma warning disable VSTHRD100

public partial class Flyout : UserControl
{
    public Flyout()
    {
        InitializeComponent();
    }

    private async void SurfaceView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FlyoutViewModel viewModel)
        {
            await viewModel.InitialRender();
        }
    }
}
