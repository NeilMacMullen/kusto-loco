using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LokqlDx.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
