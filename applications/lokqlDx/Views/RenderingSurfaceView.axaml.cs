using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }


    private RenderingSurfaceViewModel GetContext() => (DataContext as RenderingSurfaceViewModel)!;


    private void RegisterHost(ChartView chart)
    {
        if (chart.DataContext is RenderingSurfaceViewModel vm)
            vm.RegisterHost(chart);
    }

    private void ChartView_OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (sender is ChartView chart)
            RegisterHost(chart);
    }

    private void ChartView_OnDataContextChanged_OnDataContextChanged(object? sender, EventArgs e)
    {
        if (sender is ChartView chart)
            RegisterHost(chart);
    }
}
