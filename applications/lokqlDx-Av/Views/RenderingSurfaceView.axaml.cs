using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using lokqlDx;
using LokqlDx.ViewModels;
using ScottPlot;
using ScottPlot.Avalonia;

namespace LokqlDx.Views;

public partial class RenderingSurfaceView : UserControl, IDisposable
{
    private RenderingSurfaceViewModel? _vm;

    public RenderingSurfaceView()
    {
        InitializeComponent();
    }

    public void Dispose()
    {
        if (_vm is not null) _vm.PropertyChanged -= Vm_PropertyChanged;
    }


    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_vm is not null) _vm.PropertyChanged -= Vm_PropertyChanged;

        if (DataContext is RenderingSurfaceViewModel vm)
        {
            _vm = vm;
            vm.PropertyChanged += Vm_PropertyChanged;
        }
        else
        {
            _vm = null;
        }
    }

    private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RenderingSurfaceViewModel.Columns) && _vm is not null)
            DispatcherHelper.SafeInvoke(() =>
            {
                DataGrid.Columns.Clear();
                for (var i = 0; i < _vm.Columns.Count; i++)
                {
                    var column = _vm.Columns[i];
                    DataGrid.Columns.Add(new DataGridTextColumn
                    {
                        Header = column,
                        Binding = new Binding(RenderingSurfaceViewModel.Row.GetPath(i), BindingMode.OneWay)
                    });
                }
            });
    }

    //TODO -bit of a hack to tell viewmodel layer that the inner control can be used for rendering charts
    private void Control_OnLoaded(object? sender, RoutedEventArgs e) => GetContext().RegisterHost(ChartView);


    private RenderingSurfaceViewModel GetContext() => (DataContext as RenderingSurfaceViewModel)!;
 
}
