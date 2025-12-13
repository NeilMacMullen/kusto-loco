using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;
#pragma warning disable VSTHRD100

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
        DataGrid.KeyDown += DataGrid_KeyDown;
    }

    private async void DataGrid_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        var vm = DataGrid.DataContext as RenderingSurfaceViewModel;
        if (vm != null)
        {
            if ((e.KeyModifiers & KeyModifiers.Control) != 0)
            {
                if (e.Key == Key.C)
                {
                     await vm.DataGridCopyCommand.ExecuteAsync("cell");
                }
                if (e.Key == Key.R)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("row");
                }
                if (e.Key == Key.L)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("column");
                }
                if (e.Key == Key.A)
                {
                    await vm.DataGridCopyCommand.ExecuteAsync("table");
                }
            }

        }
    }
}
