using System.Collections.ObjectModel;
using BruTile.Predefined;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using Mapsui;
using NotNullStrings;
using Point = Avalonia.Point;

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly LayerManager _layerManager;
    private readonly MapArtist _mapArtist;
    private readonly TooltipManager _tooltipManager;
    [ObservableProperty] private ObservableCollection<string> _layerNames = [];

    [ObservableProperty] private Map _map;
    private KustoQueryResult _result = KustoQueryResult.Empty;
    [ObservableProperty] private bool _showLayers;

    public MapViewModel()
    {
        Map = new Map();

        Map.Widgets.Clear();
        _layerManager = new LayerManager(Map);
        LayerNames = new ObservableCollection<string>(_layerManager.GetKnown());
        _layerManager.InitialLayer(KnownTileSource.OpenStreetMap);
        _mapArtist = new MapArtist(Map);
        _tooltipManager = new TooltipManager(Map, _mapArtist.GetPinList());
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
        _mapArtist.Clear();
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
        _mapArtist.UpdateFeatures();
        Map.RefreshGraphics();
        Map.Refresh();
        ZoomToLayers(0.2, 500);
    }

    private static bool IsValid(ColumnResult res)
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
            var pin = _mapArtist.AddPin(p.Geo);
            _tooltipManager.Add(pin, p.Tooltip);
        }

        if (IsValid(indexCol))
        {
            if (IsValid(seriesCol))
            {
                var groups = points.GroupBy(p => p.Series).ToArray();
                foreach (var d in groups)
                    _mapArtist.DrawLine(d.Select(p => p.Geo).ToArray());
            }
            else
            {
                _mapArtist.DrawLine(points.Select(p => p.Geo).ToArray());
            }
        }
    }

    private static string ValOr(object?[] row, ColumnResult seriesCol, string fallback)
    {
        if (!IsValid(seriesCol))
            return fallback;
        return row[seriesCol.Index]?.ToString() ?? fallback;
    }


    private void ZoomToLayers(double paddingFraction = 0.10, int durationMs = 400)
    {
        var rects = _mapArtist.GetExtents();


        if (rects.Length == 0)
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
        => _tooltipManager.GetTooltipAtPosition(screen);
}
