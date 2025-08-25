using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

public partial class RenderingSurfaceView : UserControl, IDisposable
{
    public RenderingSurfaceView()
    {
        InitializeComponent();
    }

    public void Dispose()
    {
    }


    private void Control_OnLoaded(object? sender, RoutedEventArgs e) => RegisterHost();


    private RenderingSurfaceViewModel GetContext() => (DataContext as RenderingSurfaceViewModel)!;

    //TODO -bit of a hack to tell viewmodel layer that the inner control can be used for rendering charts
    private void RegisterHost()
    {
        if (DataContext is RenderingSurfaceViewModel vm) vm.RegisterHost(ChartView);
    }

    private void StyledElement_OnDataContextChanged(object? sender, EventArgs e) => RegisterHost();

   
}
