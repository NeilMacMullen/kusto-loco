using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using DependencyPropertyGenerator;

namespace LokqlDx.Views;

[DependencyProperty<ColumnDefinitions>(
    "ColumnDefinitions",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<RowDefinitions>(
    "RowDefinitions",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    partial void OnColumnDefinitionsChanged(ColumnDefinitions? newValue)
    {
        if (newValue is not null) MainGrid.ColumnDefinitions = newValue;
    }

    private void ColumnGridSplitter_DragCompleted(object? sender, VectorEventArgs e) =>
        ColumnDefinitions = MainGrid.ColumnDefinitions;

    partial void OnRowDefinitionsChanged(RowDefinitions? newValue)
    {
        if (newValue is not null) MainGrid.RowDefinitions = newValue;
    }

    private void RowGridSplitter_DragCompleted(object? sender, VectorEventArgs e) =>
        RowDefinitions = MainGrid.RowDefinitions;
}
