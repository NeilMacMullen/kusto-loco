using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using LokqlDx.ViewModels;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;

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


        TheMap.PointerMoved += OnMapPointerMoved;
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        var vm = DataContext as MapViewModel;
        if (vm is null || TheMap.Map is null) return;

        var screen = e.GetPosition(TheMap);

        // Screen → world
        var vp = TheMap.Map.Navigator.Viewport;
        var world = vp.ScreenToWorld(screen.X, screen.Y);

        // Pixel tolerance → world units
        const double pixelTol = 10;
        var tolWorld = vp.Resolution * pixelTol;

        var search = new MRect(
            world.X - tolWorld, world.Y - tolWorld,
            world.X + tolWorld, world.Y + tolWorld);

        // Find the pins layer
        var pinLayer = TheMap.Map.Layers.FirstOrDefault(l => l.Name == "pins") as MemoryLayer;
        if (pinLayer?.Features == null)
        {
            ToolTip.SetTip(TheMap, null);
            return;
        }

        // Prefer the nearest feature within the search rect
        var hit = pinLayer.Features
            .Where(f => f.Extent?.Intersects(search) == true)
            .OrderBy(f =>
            {
                var cx = (f.Extent!.MinX + f.Extent!.MaxX) / 2;
                var cy = (f.Extent!.MinY + f.Extent!.MaxY) / 2;
                var dx = cx - world.X;
                var dy = cy - world.Y;
                return dx * dx + dy * dy;
            })
            .FirstOrDefault();

        if (hit != null && vm.TryGetLabel(hit, out var label) && !string.IsNullOrWhiteSpace(label))
        {
            ToolTip.SetTip(TheMap, label);
        }
        else
        {
            ToolTip.SetTip(TheMap, null);
        }
    }


private void ForceUpdate()
{
    var extent = TheMap.Map.Extent;

    TheMap.Map.Navigator.ZoomToBox(extent);
    TheMap.RefreshGraphics();
}

private void InitializeComponent()
{
    AvaloniaXamlLoader.Load(this);
}
}
