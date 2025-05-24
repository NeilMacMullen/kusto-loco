using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using LokqlDx.ViewModels;
using SkiaSharp;
using Topten.RichTextKit;

namespace LokqlDx.Views;

public partial class MainView : UserControl, IDisposable
{
    public MainView()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is MainViewModel mvm)
        {
            mvm.PropertyChanged += Mvm_PropertyChanged;
        }
    }

    private void Mvm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (DataContext is MainViewModel mvm)
        {
            if (e.PropertyName == nameof(MainViewModel.ColumnDefinitions) && mvm.ColumnDefinitions is not null)
            {
                MainGrid.ColumnDefinitions = mvm.ColumnDefinitions;
            }
            else if (e.PropertyName == nameof(MainViewModel.RowDefinitions) && mvm.RowDefinitions is not null)
            {
                MainGrid.RowDefinitions = mvm.RowDefinitions;
            }
        }
    }

    public void Dispose()
    {
        if (DataContext is MainViewModel mvm)
        {
            mvm.PropertyChanged -= Mvm_PropertyChanged;
        }
    }
}
