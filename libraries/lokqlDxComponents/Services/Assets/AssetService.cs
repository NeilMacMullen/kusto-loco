using System.Text.Json;
using lokqlDxComponents.Models;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services.Assets;

public interface IAssetService
{
    bool Exists(string resourcePath);
    Stream Open(string resourcePath);
    T Deserialize<T>(string resourcePath);
    IEnumerable<string> GetAssetPathsByFolder(string resourceFolderPath);
}

public class AssetService(ILogger<AssetService> logger, IInternalAssetLoader assetLoader, AssetCatalog catalog) : IAssetService
{
    public bool Exists(string resourcePath) => catalog.Contains(resourcePath);

    public Stream Open(string resourcePath)
    {
        if (catalog.GetAsset(resourcePath) is not Asset asset)
        {
            throw new FileNotFoundException($"{resourcePath} not found", resourcePath);
        }

        var result = assetLoader.Open(asset.Uri);

        logger.LogTrace("Opened {@Asset}", asset);
        return result;
    }

    public T Deserialize<T>(string resourcePath)
    {
        using var stream = Open(resourcePath);
        var result = JsonSerializer.Deserialize<T>(stream);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    public IEnumerable<string> GetAssetPathsByFolder(string resourceFolderPath) => catalog
        .GetAssetsByFolder(resourceFolderPath)
        .Select(string (x) => x.ResourcePath);
}