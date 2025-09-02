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
using Vanara.PInvoke;

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    [ObservableProperty] private Map _map;
    private MemoryLayer _pinLayer = new();
    private MemoryLayer _lineLayer = new();
    private List<IFeature> _pinFeatures = [];
    private List<IFeature> _lineFeatures = [];
    public MapViewModel()
    {
        Map = new Map();
        Map.Layers.Add(OpenStreetMap.CreateTileLayer());
        Map.Layers.Add(_lineLayer);
        Map.Layers.Add(_pinLayer);
        _pinLayer.Features = [];
        _lineLayer.Features = [];
        _lineLayer.Features = _lineFeatures;
        _pinLayer.Features=_pinFeatures;

    }

    public void Render(KustoQueryResult result)
    {
    }

    private void Center(GeoPoint geo)
    {
        var pt = FromGeoPoint(geo);

        Map.Navigator.CenterOnAndZoomTo(pt, Map.Navigator.Resolutions[9]);
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
                    Outline = new Pen(Color.Black)
                    //Size = 20
                }
            ]
        };
        _pinFeatures.Add(pinFeature);
    }

    private MPoint FromGeoPoint(GeoPoint point) =>
        SphericalMercator.FromLonLat(point.Longitude, point.Latitude)
            .ToMPoint();

    private void DrawLine(GeoPoint start, GeoPoint end)
    {
        var coords = new[]
        {
            CoordFromGeopoint(start),
            CoordFromGeopoint(end)
        };
        var line = new LineString(coords);

        // Wrap in GeometryFeature
        var lineFeature = new GeometryFeature { Geometry = line };

        // Add style
        lineFeature.Styles.Add(new VectorStyle
        {
            Line = new Pen(Color.Blue, 4)
        });

       _lineFeatures.Add(lineFeature);
    }

    public Coordinate CoordFromGeopoint(GeoPoint geo) => new(geo.Longitude, geo.Latitude);
}

public readonly record struct GeoPoint(double Latitude, double Longitude);
