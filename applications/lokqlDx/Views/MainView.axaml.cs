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
   }
