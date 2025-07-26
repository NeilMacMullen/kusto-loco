using lokqlDxComponents.Models;

namespace lokqlDxComponents.Services.Assets;

public class AssetCatalog
{
    private readonly ILookup<string, IAsset> _folderIndex;
    private readonly OrderedDictionary<string, IAsset> _pathIndex;

    public AssetCatalog(IEnumerable<IAsset> assets)
    {
        _pathIndex = new(assets.Select(x => KeyValuePair.Create(x.ResourcePath, x)), PathComparer.Instance);
        _folderIndex = _pathIndex.Values.ToLookup(x => x.Directory, PathComparer.Instance);
    }

    public bool IsEmpty => Count is 0;
    public int Count => _pathIndex.Count;

    public IAsset GetAsset(string resourcePath) => _pathIndex.GetValueOrDefault(resourcePath) ?? IAsset.Empty;

    public IEnumerable<IAsset> GetAssetsByFolder(string folderPath) => _folderIndex[folderPath];

    public bool Contains(string resourcePath) => _pathIndex.ContainsKey(resourcePath);

    public static AssetCatalog Create(IEnumerable<string> uris) => new(uris.Select(Asset.Create));
}
