using BruTile.Predefined;
using Mapsui;
using Mapsui.Tiling.Layers;

namespace LokqlDx.ViewModels;

public class LayerManager
{
    private readonly Map Map;

    private readonly Dictionary<KnownTileSource, TileLayer> _cached = new();

    private readonly KnownTileSource[] Working =
    {
        KnownTileSource.OpenStreetMap,
        KnownTileSource.BingAerial,
        KnownTileSource.BingHybrid,
        KnownTileSource.BingRoads,
        KnownTileSource.EsriWorldTopo,
        KnownTileSource.EsriWorldPhysical,
        KnownTileSource.EsriWorldShadedRelief,
        KnownTileSource.EsriWorldTransportation,
        KnownTileSource.EsriWorldDarkGrayBase,
        KnownTileSource.BKGTopPlusColor,
        KnownTileSource.BKGTopPlusGrey
    };

    public LayerManager(Map map)
    {
        Map = map;
    }

    public string[] GetKnown() => Working.Select(l => l.ToString()).ToArray();

    private TileLayer Create(KnownTileSource tileSource)
    {
        var key = string.Empty;
        var layer = new TileLayer(
            KnownTileSources.Create(
                tileSource,
                key));
        _cached.Add(tileSource, layer);
        return layer;
    }
    public void FetchLayer(string name)
    {
        var enumValue = Enum.Parse<KnownTileSource>(name);
        if (!_cached.TryGetValue(enumValue, out var layer))
        {
            layer = Create(enumValue);
        }

        var prev = Map.Layers.Get(0);
        Map.Layers.Insert(0, layer);
        Map.Layers.Remove(prev);
    }

    public void InitialLayer(KnownTileSource initialLayer)
    {
        var layer = Create(initialLayer);
        Map.Layers.Insert(0, layer);
    }
}
