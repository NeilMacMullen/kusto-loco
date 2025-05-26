using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using lokqlDx;
using LokqlDx.ViewModels;
using ScottPlot;

namespace LokqlDx.Views;

public partial class RenderingSurfaceView : UserControl, IDisposable, IScottPlotHost
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
                        Binding = new Binding(RenderingSurfaceViewModel.Row.GetPath(i))
                    });
                }
            });
    }

    private void Control_OnLoaded(object? sender, RoutedEventArgs e) => GetContext().RegisterHost(this);


    private RenderingSurfaceViewModel GetContext() => (DataContext as RenderingSurfaceViewModel)!;

    /// <summary>
    ///     Scottplot doesn't support binding so we treat the view as a host
    /// </summary>

    #region IScottplotHost

    public Plot GetPlot(bool reset)
    {
        return DispatcherHelper.SafeInvoke(() =>
        {
            if (reset)
                AvaPlot1.Reset();
            return AvaPlot1.Plot;
        });
    }

    public void FinishUpdate() =>
        DispatcherHelper.SafeInvoke(() =>
            AvaPlot1.Refresh());

    public void CopyToClipboard()
    {
        try
        {
            var bytes = AvaPlot1.Plot.GetImageBytes((int)AvaPlot1.Width,
                (int)AvaPlot1.Height, ImageFormat.Png);
            using var memoryStream = new MemoryStream(bytes);
            {
                //TODO - not sure how to implement avalonia clipboard support
            }
        }
        catch
        {
        }
    }

    #endregion
}
