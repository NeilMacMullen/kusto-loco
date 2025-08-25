using Avalonia.Controls;
using Avalonia.Interactivity;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;
#pragma warning disable VSTHRD100
public partial class ResultDisplayView : UserControl
{
    public ResultDisplayView()
    {
        InitializeComponent();
    }
    private async void SurfaceView_OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is FlyoutViewModel viewModel) await viewModel.InitialRender();
    }
}
