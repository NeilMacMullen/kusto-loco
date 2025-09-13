using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;
using Vanara.PInvoke;

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


    private void RegisterHost(ChartView chart)
    {
        if (chart.DataContext is RenderingSurfaceViewModel vm)
        {
            var newState = new RegistrationState(vm, chart);
           // if (_registrationState == newState)
//return;
        
            vm.RegisterHost(chart);
            _registrationState=newState;
        }
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
    private RegistrationState _registrationState = new(null,null);
    private record struct RegistrationState(RenderingSurfaceViewModel? ViewModel,ChartView? ChartView);
}
