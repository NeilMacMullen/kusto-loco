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

    private static KnownTileSource Parse(string name)
    {
        if (Enum.TryParse<KnownTileSource>(name,true,out var known))
            return known;
        return KnownTileSource.OpenStreetMap;
    }
    public void FetchLayer(string name)
    {
        var enumValue = Parse(name);
        if (!_cached.TryGetValue(enumValue, out var layer))
        {
            layer = Create(enumValue);
        }

        var prev = Map.Layers.Get(0);
        Map.Layers.Insert(0, layer);
        Map.Layers.Remove(prev);
    }

    public void InitialLayer(string initialLayer)
    {
        var enumValue = Parse(initialLayer);
        var layer = Create(enumValue);
        Map.Layers.Insert(0, layer);
    }
}
