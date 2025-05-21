using Avalonia.Controls;
using LokqlDx.ViewModels;

namespace LokqlDx.Views;

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
    }
}
