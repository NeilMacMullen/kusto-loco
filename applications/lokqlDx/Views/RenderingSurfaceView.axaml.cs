using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;

#pragma warning disable VSTHRD100

namespace LokqlDx.Views;

public partial class RenderingSurfaceView : UserControl, IDisposable
{
    private TreeDataGrid? _dataGrid;
    
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
        _dataGrid = this.FindControl<TreeDataGrid>("DataGrid");
        if (_dataGrid != null)
        {
            _dataGrid.KeyDown += DataGrid_KeyDown;
        }
    }

    private async void DataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        var vm = _dataGrid?.DataContext as RenderingSurfaceViewModel;
        if (vm != null)
            if ((e.KeyModifiers & KeyModifiers.Control) != 0)
            {
                if (e.Key == Key.C)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("cell");
                    e.Handled = true;
                }

                if (e.Key == Key.R)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("row");
                    e.Handled = true;
                }

                if (e.Key == Key.L)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("column");
                    e.Handled = true;
                }

                if (e.Key == Key.A)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("table");
                    e.Handled = true;
                }
            }
    }
}
