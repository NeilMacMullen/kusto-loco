using Avalonia;
using Avalonia.Controls;
using LokqlDx.ViewModels;
using DependencyPropertyGenerator;
using Avalonia.Media;

namespace LokqlDx.Views;

[DependencyProperty<Point>(
    "WindowPosition",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<Size>(
    "WindowSize",
    DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindow(MainView view)
    {
        InitializeComponent();
        Content = view;

        if (ActualTransparencyLevel == WindowTransparencyLevel.Mica)
        {
            Background = Brushes.Transparent;
        }
    }

    private void Window_PositionChanged(object? sender, PixelPointEventArgs e)
    {
        WindowPosition = new Point(e.Point.X, e.Point.Y);
    }

    partial void OnWindowPositionChanged(Point newValue)
    {
        this.Position = new PixelPoint((int)newValue.X, (int)newValue.Y);
    }

    private void Window_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        WindowSize = new Size(e.NewSize.Width, e.NewSize.Height);
    }

    partial void OnWindowSizeChanged(Size newValue)
    {
        Width = newValue.Width;
        Height = newValue.Height;
    }
}
