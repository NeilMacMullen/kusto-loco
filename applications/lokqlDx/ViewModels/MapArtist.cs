using KustoLoco.Core.Settings;
using Mapsui;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;

namespace LokqlDx.ViewModels;

public class MapArtist
{
    private readonly KustoSettingsProvider _settings;
    private readonly List<IFeature> _lineFeatures = [];
    private readonly MemoryLayer _lineLayer = new("lines");

    private readonly List<IFeature> _pinFeatures = [];
    private readonly MemoryLayer _pinLayer = new("pins");

    public MapArtist(Map Map,KustoSettingsProvider settings)
    {
        _settings = settings;
        _lineLayer.Features = _lineFeatures;
        _lineLayer.Style = null;
        _pinLayer.Style = null;
        _lineLayer.Enabled = true;
        _pinLayer.Enabled = true;
        _pinLayer.Features = _pinFeatures;

        Map.Layers.Add(_lineLayer);
        Map.Layers.Add(_pinLayer);
    }

    public List<IFeature> GetPinList()
    {
        return _pinFeatures;
    }

    public void Clear()
    {
        _lineFeatures.Clear();
        _pinFeatures.Clear();
    }

    public void UpdateFeatures()
    {
        _pinLayer.Features = _pinFeatures.ToArray();
        _lineLayer.Features = _lineFeatures.ToArray();
        _pinLayer.DataHasChanged();
        _lineLayer.DataHasChanged();
    }

    public IFeature AddPin(GeoPoint geo,
        double scale,
        SymbolType symbol,
        Brush pinFill)
    {
        var pt = MapGeometry.FromGeoPoint(geo);

        var pinFeature = new PointFeature(pt)
        {
            Styles =
            [
                new SymbolStyle
                {
                    SymbolType = symbol,
                    Fill = pinFill,
                    Outline = new Pen(Color.Black),
                    SymbolScale = scale
                }
            ]
        };


        _pinFeatures.Add(pinFeature);
        return pinFeature;
    }

    public void DrawLine(GeoPoint[] points,Color penColor)
    {
        DrawPolygon(points,penColor);
        return;
        /*
        if (points.Length < 2)
            return;
        var coords = points.Select(MapGeometry.CoordFromGeopoint).ToArray();
        var line = new LineString(coords);

        // Wrap in GeometryFeature
        var lineFeature = new GeometryFeature { Geometry = line };

        // Add style
        lineFeature.Styles.Add(new VectorStyle
        {
            Line = new Pen(penColor)
        });

        _lineFeatures.Add(lineFeature);
        */
    }

    public void DrawPolygon(GeoPoint[] points, Color fillColor, Color? outlineColor = null, float outlineWidth = 1f)
    {
        if (points == null || points.Length < 3)
            return; // need at least 3 vertices

        // Convert to coordinates
        var coords = points.Select(MapGeometry.CoordFromGeopoint).ToList();

        // Ensure first and last are the same (LinearRing requires closure)
        // Equals2D compares X/Y without Z/M, which is what we want here.
        if (!coords[0].Equals2D(coords[^1]))
            coords.Add(coords[0]);

        // LinearRing requires Coordinate[]
        var ring = new LinearRing(coords.ToArray());
        var polygon = new Polygon(ring);

        var feature = new GeometryFeature { Geometry = polygon };

        feature.Styles.Add(new VectorStyle
        {
            Fill = new Brush(fillColor),                     // e.g. fillColor.WithAlpha(128) for translucency
            Outline = outlineColor.HasValue ? new Pen(outlineColor.Value, outlineWidth) : null
        });

        _lineFeatures.Add(feature);
    }



    public MRect[] GetExtents()
    {
        return new[] { _pinLayer.Extent, _lineLayer.Extent }.OfType<MRect>().ToArray();
    }
}
