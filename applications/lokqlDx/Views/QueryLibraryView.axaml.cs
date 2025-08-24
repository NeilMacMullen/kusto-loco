using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using LokqlDx.ViewModels;
using System.Globalization;

namespace LokqlDx.Views;

public partial class QueryLibraryView : UserControl
{
    public QueryLibraryView()
    {
        InitializeComponent();
    }
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
