using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using LokqlDx.ViewModels;
using SkiaSharp;
using DependencyPropertyGenerator;

namespace LokqlDx.Views;

[DependencyProperty<ColumnDefinitions>(
    "ColumnDefinitions",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<RowDefinitions>(
    "RowDefinitions",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
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

    partial void OnColumnDefinitionsChanged(ColumnDefinitions? newValue)
    {
        if (newValue is not null)
        {
            MainGrid.ColumnDefinitions = newValue;
        }
    }

    private void ColumnGridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        ColumnDefinitions = MainGrid.ColumnDefinitions;
    }

    partial void OnRowDefinitionsChanged(RowDefinitions? newValue)
    {
        if (newValue is not null)
        {
            MainGrid.RowDefinitions = newValue;
        }
    }

    private void RowGridSplitter_DragCompleted(object? sender, Avalonia.Input.VectorEventArgs e)
    {
        RowDefinitions = MainGrid.RowDefinitions;
    }
}
