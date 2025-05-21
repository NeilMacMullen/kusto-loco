using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using LokqlDx.ViewModels;
using System.ComponentModel;

namespace LokqlDx.Views;

public partial class RenderingSurfaceView : UserControl, IDisposable
{
    RenderingSurfaceViewModel? _vm;
    public RenderingSurfaceView()
    {
        InitializeComponent();
    }


    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_vm is not null)
        {
            _vm.PropertyChanged -= Vm_PropertyChanged;
        }

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
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                DataGrid.Columns.Clear();
                for (int i = 0; i < _vm.Columns.Count; i++)
                {
                    string? column = _vm.Columns[i];
                    DataGrid.Columns.Add(new DataGridTextColumn()
                    {
                        Header = column,
                        Binding = new Binding(RenderingSurfaceViewModel.Row.GetPath(i)),
                    });
                }
            });
        }
    }

    public void Dispose()
    {
        if (_vm is not null)
        {
            _vm.PropertyChanged -= Vm_PropertyChanged;
        }
    }
}
