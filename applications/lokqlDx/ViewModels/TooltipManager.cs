using Avalonia;
using Mapsui;
using Mapsui.Extensions;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public class TooltipManager
{
    private readonly Map _map;
    private readonly List<IFeature> _pins;


    private Dictionary<IFeature, string> _featureLabels = new();

    public TooltipManager(Map map, List<IFeature> pins)
    {
        _map = map;
        _pins = pins;
    }

    public bool TryGetLabel(IFeature feature, out string label) =>
        _featureLabels.TryGetValue(feature, out label!);

    public void Clear()
    {
        _featureLabels = [];
    }

    public string GetTooltipAtPosition(Point screen)
    {
        // Screen → world
        var vp = _map.Navigator.Viewport;
        var world = vp.ScreenToWorld(screen.X, screen.Y);

        // Pixel tolerance → world units
        const double pixelTol = 10;
        var tolWorld = vp.Resolution * pixelTol;

        var search = new MRect(
            world.X - tolWorld, world.Y - tolWorld,
            world.X + tolWorld, world.Y + tolWorld);

        // Prefer the nearest feature within the search rect
        var hit = _pins
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

        if (hit != null && TryGetLabel(hit, out var label) && !string.IsNullOrWhiteSpace(label)) return label;

        return string.Empty;
    }

    public void Add(IFeature pinFeature, string label)
    {
        if (label.IsBlank())
            return;
        _featureLabels[pinFeature] = label;
    }
}
