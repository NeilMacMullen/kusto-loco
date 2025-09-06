using System.Collections.ObjectModel;
using BruTile.Predefined;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Mapsui;
using Markdig.Renderers;
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

    MapResultRenderer _resultRenderer;
    public MapViewModel(KustoSettingsProvider settings)
    {
        Map = new Map();
        Map.Widgets.Clear();
        _layerManager = new LayerManager(Map);
        LayerNames = new ObservableCollection<string>(_layerManager.GetKnown());
        
        _layerManager.InitialLayer(settings.GetOr("map.defaultLayer", $"{KnownTileSource.OpenStreetMap}"));
        _mapArtist = new MapArtist(Map,settings);
        _tooltipManager = new TooltipManager(Map, _mapArtist.GetPinList());
        _resultRenderer = new MapResultRenderer(settings, _mapArtist, _tooltipManager);
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
        _resultRenderer.Populate(result);
        _mapArtist.UpdateFeatures();
        Map.RefreshGraphics();
        Map.Refresh();
        ZoomToLayers(0.2, 500);
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

    public Action OnCopyToClipboard = () => { };

    public void CopyToClipboard()
    {
        OnCopyToClipboard();
    }
}
