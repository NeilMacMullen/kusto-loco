using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace LokqlDx.Views;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
        TheMap.LayoutUpdated += (_, _) =>
        {
            if (TheMap.IsEffectivelyVisible)
            {
              //  ForceUpdate();
            }
        };
        TheMap.AttachedToVisualTree += (_, _) => { ForceUpdate(); };
        TheMap.DoubleTapped += TheMap_DoubleTapped;
       

    }

    private void TheMap_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        
    }

    private void ForceUpdate()
    {
        var extent = TheMap.Map.Extent;

        TheMap.Map.Navigator.ZoomToBox(extent);
        TheMap.RefreshGraphics();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
