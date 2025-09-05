using BruTile.Predefined;
using BruTile.Wms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using NotNullStrings;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using IFeature = Mapsui.IFeature;
using Point = Avalonia.Point;

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly MemoryLayer _ellipseLayer = new("ellipses");
    private readonly List<IFeature> _ellipsFeatures = [];
  
    private readonly List<IFeature> _lineFeatures = [];
    private readonly MemoryLayer _lineLayer = new("lines");

    private readonly List<IFeature> _pinFeatures = [];
    private readonly MemoryLayer _pinLayer = new("pins");
    private readonly LayerManager _layerManager;
    private readonly TooltipManager _tooltipManager;
    [ObservableProperty] private ObservableCollection<string> _layerNames = [];

    [ObservableProperty] private Map _map;
    [ObservableProperty] private bool _showLayers;
    private KustoQueryResult _result = KustoQueryResult.Empty;

    public MapViewModel()
    {
        Map = new Map();
        _lineLayer.Features = _lineFeatures;
        _lineLayer.Style = null;
        _pinLayer.Style = null;
        _lineLayer.Enabled = true;
        _pinLayer.Enabled = true;
        _pinLayer.Features = _pinFeatures;
        _ellipseLayer.Enabled = true;
        _ellipseLayer.Features = _ellipsFeatures;

        Map.Layers.Add(_lineLayer);
        Map.Layers.Add(_pinLayer);
        Map.Layers.Add(_ellipseLayer);
        Map.Widgets.Clear();
        _layerManager = new LayerManager(Map);
        LayerNames = new ObservableCollection<string>(_layerManager.GetKnown());
        _layerManager.InitialLayer(KnownTileSource.OpenStreetMap);
        _tooltipManager = new TooltipManager(Map, _pinFeatures);
    }

    [RelayCommand]
    public void ToggleLayers()
    {
        ShowLayers = !ShowLayers;
    }

    [RelayCommand]
    public void ChangeLayer(string wantedLayer)
    {
        _layerManager.FetchLayer(wantedLayer);
        ShowLayers = false;
    }

    private void Reset()
    {
        _lineFeatures.Clear();
        _pinFeatures.Clear();
        _tooltipManager.Clear();
    }


    private ColumnResult FindColumn(string names)
    {
        foreach (var name in names.Tokenize(","))
        {
            var matches = _result.ColumnDefinitions()
                .Where(d => d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            if (matches.Any())
                return matches.First();
        }

        return new ColumnResult(string.Empty, -1, typeof(object));
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
        ZoomToLayers(0.2, 500);
    }

    private bool IsValid(ColumnResult res)
        => res.Index >= 0;

    private void Populate()
    {
        var latCol = FindColumn("latitude,lat");
        var lonCol = FindColumn("longitude,lon");
        var sizeCol = FindColumn("size,radius");
        var seriesCol = FindColumn("series");
        var indexCol = FindColumn("index,timestamp");
        var tooltipCol = FindColumn("tooltip");

        //we need at least latitude and longitude
        if (!IsValid(latCol) || !IsValid(lonCol))
            return;

        var points = _result.EnumerateRows()
            .Select(row => new
            {
                Geo = GeoPoint.Maybe(row[latCol.Index],
                    row[lonCol.Index]),
                Radius = double.TryParse(ValOr(row, sizeCol, "0"), out var rad) ? rad : 0.0,
                Tooltip = ValOr(row, tooltipCol, string.Empty),
                Series = ValOr(row, seriesCol, string.Empty)
            })
            .Where(g => g.Geo.Valid)
            .ToArray();

        foreach (var p in points)
            //   if (IsValid(sizeCol))
            //     DrawEllipseMeters(p.Geo, p.Radius);
            // else
            AddPin(p.Geo, p.Tooltip);

        if (IsValid(indexCol))
        {
            if (IsValid(seriesCol))
            {
                var groups = points.GroupBy(p => p.Series).ToArray();
                foreach (var d in groups) DrawLine(d.Select(p => p.Geo).ToArray());
            }
            else
            {
                DrawLine(points.Select(p => p.Geo).ToArray());
            }
        }
    }

    private string ValOr(object?[] row, ColumnResult seriesCol, string fallback)
    {
        if (!IsValid(seriesCol))
            return fallback;
        return row[seriesCol.Index]?.ToString() ?? fallback;
    }

    private MPoint FromGeoPoint(GeoPoint point) =>
        SphericalMercator.FromLonLat(point.Longitude, point.Latitude)
            .ToMPoint();

    private void AddPin(GeoPoint geo, string label)
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
                    SymbolScale = 0.3
                }
            ]
        };

        if (label.IsNotBlank())
            _tooltipManager.Add(pinFeature,label);

        _pinFeatures.Add(pinFeature);
    }


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

    public string GetTooltipAtPosition(Point screen)
    {
        return _tooltipManager.GetTooltipAtPosition(screen);

    }
}

public class MapArtist
{

}
