using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using Mapsui.Projections;
using Mapsui.Nts;

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly List<IFeature> _lineFeatures = [];
    private readonly List<IFeature> _ellipsFeatures = [];
    private MemoryLayer _lineLayer = new("lines");
    private MemoryLayer _ellipseLayer = new("ellipses");

    [ObservableProperty] private Map _map;
    private readonly List<IFeature> _pinFeatures = [];
    private MemoryLayer _pinLayer = new("pins");
    private KustoQueryResult _result = KustoQueryResult.Empty;
    private readonly Dictionary<IFeature, string> _featureLabels = new();

    public MapViewModel()
    {
        Map = new Map();
        Map.Layers.Add(OpenStreetMap.CreateTileLayer());

        _lineLayer.Features = _lineFeatures;
        _lineLayer.Style = null;
        _pinLayer.Style = null;
        _lineLayer.Enabled = true;
        _pinLayer.Enabled = true;
        _pinLayer.Features = _pinFeatures;
        _ellipseLayer.Features = _ellipsFeatures;

        Map.Layers.Add(_lineLayer);
        Map.Layers.Add(_pinLayer);
        Map.Layers.Add(_ellipseLayer);
        Map.Widgets.Clear();
    }

    public bool TryGetLabel(IFeature feature, out string label) =>
        _featureLabels.TryGetValue(feature, out label!);

    private void Reset()
    {
        _lineFeatures.Clear();
        _pinFeatures.Clear();
    }

    private void DrawEllipseMeters(double centerLat, double centerLon, double radiusXmeters, double radiusYmeters,
        Color? fillColor = null, Color? outlineColor = null)
    {
        // 1. Convert center from lat/lon to Web Mercator meters
        var merc = SphericalMercator.FromLonLat(centerLon, centerLat);
        var center = new Coordinate(merc.x, merc.y);

        // 2. Create a unit circle around (0,0)
        var circle = new Point(0, 0).Buffer(1, 64); // 64 segments for smoothness

        // 3. Scale to desired radii in meters
        var transform = new AffineTransformation();
        transform.Scale(radiusXmeters, radiusYmeters);
        transform.Translate(center.X, center.Y);
        var ellipse = transform.Transform(circle);

        // 4. Create a feature for the ellipse
        var ellipseFeature = new GeometryFeature { Geometry = ellipse };

        // 5. Style it
        ellipseFeature.Styles.Add(new VectorStyle
        {
            Fill = new Brush(fillColor ?? Color.FromArgb(64, 0, 0, 255)), // semi‑transparent blue by default
            Outline = new Pen(outlineColor ?? Color.Blue, 2)
        });

        // 6. Add to a layer (create one if needed)


       
    

    _ellipsFeatures.Add(ellipseFeature);
}

public void Render(KustoQueryResult result)

    {
        _result = result;
        Reset();
        Populate();
        _pinLayer.Features = _pinFeatures.ToArray();
        _lineLayer.Features = _lineFeatures.ToArray();
        _pinLayer.DataHasChanged();
        _lineLayer.DataHasChanged();
        _ellipseLayer.DataHasChanged();

        Map.RefreshGraphics();
        Map.Refresh();
       ZoomToLayers(0.2,500);
    }

    private void Populate()
    {
        double ToD(object? o)
        {
            return o is double d ? d : 0.0;
        }

        var points = _result.EnumerateRows()
            .Select(row => new
            {
                Geo = new GeoPoint(ToD(row[0]),
                    ToD(row[1])),

                Series = _result.ColumnCount >= 4 ? row[3] : 0
            }).ToArray();
        foreach (var p in points)
            AddPin(p.Geo);
        if (_result.ColumnCount == 3)
            DrawLine(points.Select(p => p.Geo).ToArray());
        if (_result.ColumnCount == 4)
        {
            var groups = points.GroupBy(p => p.Series).ToArray();
            foreach (var d in groups)
            {
                DrawLine(d.Select(p => p.Geo).ToArray());
            }
        }
    }


    private void AddPin(GeoPoint geo)
    {
        var pt = FromGeoPoint(geo);

        var pinFeature = new PointFeature(pt)
        {
            Styles =
            [
                new SymbolStyle
                {
                    SymbolType = SymbolType.Ellipse,
                    Fill = new Brush(Color.Red),
                    Outline = new Pen(Color.Black),
                    SymbolScale = 1
                }
            ]
        };
        var label = "TOOLTIP!";
        var text = label ?? $"{geo.Latitude:F4}, {geo.Longitude:F4}";
        _featureLabels[pinFeature] = text;

        _pinFeatures.Add(pinFeature);
    }



    private MPoint FromGeoPoint(GeoPoint point) =>
        SphericalMercator.FromLonLat(point.Longitude, point.Latitude)
            .ToMPoint();

    private void DrawLine(GeoPoint[] points)
    {
        var coords = points.Select(CoordFromGeopoint).ToArray();
        var line = new LineString(coords);

        // Wrap in GeometryFeature
        var lineFeature = new GeometryFeature { Geometry = line };

        // Add style
        lineFeature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Color.Blue)
        });

        _lineFeatures.Add(lineFeature);
    }

    public Coordinate CoordFromGeopoint(GeoPoint geo)
    {
        var merc = SphericalMercator.FromLonLat(geo.Longitude, geo.Latitude);
        return new Coordinate(merc.x, merc.y);
    }
    private void ZoomToLayers(double paddingFraction = 0.10, int durationMs = 400)
    {
        var rects = new[] { _pinLayer.Extent, _lineLayer.Extent }
            .Where(e => e != null)
            .OfType<MRect>()
            .ToList();

        if (rects.Count == 0)
            return;

        var extent = rects[0];
        foreach (var r in rects.Skip(1))
            extent = extent.Join(r);

        if (extent.Width <= 0 || extent.Height <= 0)
        {
            var pad = 500;
            extent = extent.Grow(pad, pad);
        }
        else if (paddingFraction > 0)
        {
            extent = extent.Grow(extent.Width * paddingFraction, extent.Height * paddingFraction);
        }

        Map.Navigator.ZoomToBox(extent, duration: durationMs);
    }


}

public readonly record struct GeoPoint(double Latitude, double Longitude);
