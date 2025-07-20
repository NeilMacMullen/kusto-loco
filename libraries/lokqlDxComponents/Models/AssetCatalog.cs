using Intellisense.FileSystem.Paths;

namespace lokqlDxComponents.Models;

public class AssetCatalog
{
    private readonly ILookup<string, Asset> _folders;
    private readonly Dictionary<string, Asset> _resourcePaths;

    public AssetCatalog(IEnumerable<Asset> assets)
    {
        _resourcePaths = assets.ToDictionary(x => x.ResourcePath);
        _folders = _resourcePaths.Values.ToLookup(x => x.Folder);
    }

    public bool IsEmpty => !_resourcePaths.Any();
    public int Count => _resourcePaths.Count;

    public Asset GetAsset(string resourcePath) => _resourcePaths.GetValueOrDefault(resourcePath.NormalizePath(), Asset.Empty);

    public IEnumerable<Asset> GetAssetsByFolder(string folderPath) => _folders[folderPath.NormalizePath()];

    public bool Contains(string resourcePath) => _resourcePaths.ContainsKey(resourcePath.NormalizePath());
}
