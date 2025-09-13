using System.Collections.ObjectModel;
using BruTile.Predefined;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using Mapsui;
using Point = Avalonia.Point;

namespace LokqlDx.ViewModels;

public partial class MapViewModel : ObservableObject
{
    private readonly LayerManager _layerManager;
    private readonly MapArtist _mapArtist;
    private const string MapCurrentLayerPreference = "map.currentLayerPreference";

    private readonly MapResultRenderer _resultRenderer;
    private readonly KustoSettingsProvider _settings;
    private readonly TooltipManager _tooltipManager;
    [ObservableProperty] private ObservableCollection<string> _layerNames = [];

    [ObservableProperty] private Map _map;
    [ObservableProperty] private bool _showLayers;

    [ObservableProperty] private bool _show;
    public void Activate(bool onOff) => Show = onOff;
    public Action OnCopyToClipboard = () => { };

    public MapViewModel(KustoSettingsProvider settings)
    {
        _settings = settings;
        Map = new Map();
        Map.Widgets.Clear();
        _layerManager = new LayerManager(Map);
        LayerNames = new ObservableCollection<string>(_layerManager.GetKnown());

        if (settings.HasSetting(MapCurrentLayerPreference))
            _layerManager.InitialLayer(settings.GetOr(MapCurrentLayerPreference,
                $"{KnownTileSource.OpenStreetMap}"));
        else
            _layerManager.InitialLayer(settings.GetOr("map.defaultLayer", $"{KnownTileSource.OpenStreetMap}"));
        _mapArtist = new MapArtist(Map, settings);
        _tooltipManager = new TooltipManager(Map, _mapArtist.GetPinList());
        _resultRenderer = new MapResultRenderer(settings, _mapArtist, _tooltipManager);
    }

    [RelayCommand]
    public void ToggleLayers() => ShowLayers = !ShowLayers;

    [RelayCommand]
    public void ChangeLayer(string wantedLayer)
    {
        _layerManager.FetchLayer(wantedLayer);
        _settings.Set(MapCurrentLayerPreference, wantedLayer);
        ShowLayers = false;
    }

    private void Reset()
    {
        _mapArtist.Clear();
        _tooltipManager.Clear();
    }


    public void Render(KustoQueryResult result)

    {
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

    public void CopyToClipboard() => OnCopyToClipboard();
}
