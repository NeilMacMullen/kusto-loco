using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core;
using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Projections;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using NetTopologySuite.Geometries;
using NotNullStrings;
using IFeature = Mapsui.IFeature;

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly MemoryLayer _ellipseLayer = new("ellipses");
    private readonly List<IFeature> _ellipsFeatures = [];
    private readonly Dictionary<IFeature, string> _featureLabels = new();
    private readonly List<IFeature> _lineFeatures = [];
    private readonly MemoryLayer _lineLayer = new("lines");
    private readonly List<IFeature> _pinFeatures = [];
    private readonly MemoryLayer _pinLayer = new("pins");

    [ObservableProperty] private Map _map;
    private KustoQueryResult _result = KustoQueryResult.Empty;

    public MapViewModel()
    {
        Map = new Map();
        // Map.Layers.Add(OpenStreetMap.CreateTileLayer());

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

        // Define the tile schema (Web Mercator)
        var schema = new GlobalSphericalMercator();

        // Create the URL builder
        var urlBuilder = new BasicUrlBuilder(
            "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}" // optional subdomains array, e.g. new[] { "a", "b", "c" }
        );

        // Create the tile source
        var tileSource = new HttpTileSource(
            schema,
            urlBuilder,
            "Esri World Imagery",
            attribution: new Attribution("Source: Esri, Maxar, Earthstar Geographics, and the GIS User Community")
        );

        // Wrap in a layer
        var esriLayer = new TileLayer(tileSource) { Name = "Esri World Imagery" };

        // Add to your map
        Map.Layers.Insert(0, esriLayer);
    }

    public bool TryGetLabel(IFeature feature, out string label) =>
        _featureLabels.TryGetValue(feature, out label!);

    private void Reset()
    {
        _lineFeatures.Clear();
        _pinFeatures.Clear();
    }
    
    /*
    private void AddCircleMeters(GeoPoint center, double radiusMeters, double minPixelSize = 10, int segments = 64)
    {
        var mpp = Map.Navigator.Viewport.Resolution; // meters per pixel
        var effectiveRadiusMeters = Math.Max(radiusMeters, minPixelSize * mpp);

        // This returns a projected Mapsui.Geometries.Point in Web Mercator
        var centerProjected = SphericalMercator.FromLonLat(center.Longitude, center.Latitude);

        var polygon = BuildCirclePolygon(centerProjected, effectiveRadiusMeters, segments);

        var feature = new GeometryFeature(polygon)
        {
            Styles =
            {
                new VectorStyle
                {
                    Fill = new Brush(Color.FromArgb(64, 0, 0, 255)),
                    Outline = new Pen(Color.Blue, 2)
                }
            }
        };

        _ellipsFeatures.Add(feature);
    }

    private Polygon BuildCirclePolygon(
        Point center,
        double radiusMeters,
        int segments)
    {
        var coords = new Coordinate[segments + 1];

        for (var i = 0; i < segments; i++)
        {
            var angle = i * 2 * Math.PI / segments;
            var x = center.X + radiusMeters * Math.Cos(angle);
            var y = center.Y + radiusMeters * Math.Sin(angle);
            coords[i] = new Coordinate(x, y);
        }

        coords[segments] = coords[0]; // close ring

        var ring = new LinearRing(coords);
        return new Polygon(ring);
    }
    */

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
        {
         //   if (IsValid(sizeCol))
           //     DrawEllipseMeters(p.Geo, p.Radius);
           // else
                AddPin(p.Geo, p.Tooltip);
        }

        if (IsValid(indexCol))
        {
            if (IsValid(seriesCol))
            {
                var groups = points.GroupBy(p => p.Series).ToArray();
                foreach (var d in groups)
                {
                    DrawLine(d.Select(p => p.Geo).ToArray());
                }
            }
            else DrawLine(points.Select(p => p.Geo).ToArray());
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
            _featureLabels[pinFeature] = label;

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
}

public class GeoPoint(double lat, double lon, bool valid)
{
    public readonly double Latitude = lat;
    public readonly double Longitude = lon;
    public readonly bool Valid = valid;

    public GeoPoint(double latitude, double longitude) : this(latitude, longitude, true)
    {
    }

    public GeoPoint LatLon(double lat, double lon) => new(lat, lon, true);

    public static GeoPoint Maybe(object? lat, object? lon)
    {
        if (lat is not double dlat || lon is not double dlon)
            return new GeoPoint(0, 0, false);
        return new GeoPoint(dlat, dlon, true);
    }
}
