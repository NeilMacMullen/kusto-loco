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

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly List<IFeature> _lineFeatures = [];
    private MemoryLayer _lineLayer = new();
    [ObservableProperty] private Map _map;
    private readonly List<IFeature> _pinFeatures = [];
    private MemoryLayer _pinLayer = new();
    private KustoQueryResult _result = KustoQueryResult.Empty;

    public MapViewModel()
    {
        Map = new Map();

        Map = new Map();
        Map.Layers.Add(OpenStreetMap.CreateTileLayer());

        _lineLayer.Features = _lineFeatures;
        _lineLayer.Style = null;
        _pinLayer.Style = null;
        _lineLayer.Enabled = true;
        _pinLayer.Enabled = true;
        _pinLayer.Features = _pinFeatures;
        Map.Layers.Add(_lineLayer);
        Map.Layers.Add(_pinLayer);
        Map.Widgets.Clear();
    }

    public void Alt()
    {
        Map.Layers.Add(OpenStreetMap.CreateTileLayer());

        _pinLayer = new MemoryLayer { Name = "PinLayer", Enabled = true };
        _lineLayer = new MemoryLayer { Name = "LineLayer", Enabled = true };


        var line = new LineString(new[]
        {
            new Coordinate(SphericalMercator.FromLonLat(-1, -1).x, SphericalMercator.FromLonLat(-1, -1).y),
            new Coordinate(SphericalMercator.FromLonLat(1, 1).x, SphericalMercator.FromLonLat(1, 1).y)
        });

        var lineFeature = new GeometryFeature { Geometry = line };
        lineFeature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Color.Blue, 4)
        });


        _lineLayer.Features = [lineFeature];

        Map.Layers.Add(_lineLayer);
        Map.Layers.Add(_pinLayer);
    }

    private void Reset()
    {
        _lineFeatures.Clear();
        _pinFeatures.Clear();
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
                    SymbolScale = 0.1
                }
            ]
        };
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
