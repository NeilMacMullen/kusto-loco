using System.Reflection;
using System.Text.Json;
using lokqlDxComponents.Exceptions;
using lokqlDxComponents.Models;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public interface IAssetService
{
    bool Exists(string resourcePath);
    Stream Open(string resourcePath);
    T Deserialize<T>(string resourcePath);
    IEnumerable<string> GetAssetPathsByFolder(string resourceFolderPath);
}

public class AssetService(ILogger<AssetService> logger, IInternalAssetLoader assetLoader) : IAssetService
{
    private AssetCatalog? _catalog;
    private AssetCatalog Catalog => _catalog ??= CreateCatalog();


    private AssetCatalog CreateCatalog()
    {
        // AssetLoader retrieves assets from compiled binary.
        // If that fails, there is either a fundamental issue at compilation or incorrect URI configured, which we cannot recover from.

        var thisAsm = Assembly.GetExecutingAssembly().GetName().Name!;
        var assemblies = AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(asm => asm.GetReferencedAssemblies().Any(refAsm => refAsm.Name == thisAsm))
            .Select(asm => asm.GetName().Name)
            .OfType<string>()
            .Append(thisAsm)
            .ToList();


        var assets = assemblies
            .SelectMany(assemblyName => assetLoader
                .GetAssets(assemblyName.ToBaseUri())
                .Select(x => new Asset { AssemblyName = assemblyName, ResourcePath = x.AbsolutePath })
            )
            .DistinctBy(x => x.ResourcePath);

        var catalog = new AssetCatalog(assets);

        logger.LogInformation("Initialized asset catalog with {AssetCount} assets", catalog.Count);

        if (catalog.IsEmpty)
        {
            throw new CompilationException("No assets found in any of the assemblies specified")
            {
                Data =
                {
                    ["Assemblies"] = assemblies
                }
            };
        }

        return catalog;
    }

    public bool Exists(string resourcePath) => Catalog.Contains(resourcePath);

    public Stream Open(string resourcePath)
    {
        var asset = Catalog.GetAsset(resourcePath);
        if (asset.IsEmpty)
        {
            throw new FileNotFoundException(asset.AssemblyName);
        }

        var result = assetLoader.Open(asset.ToUri());

        logger.LogTrace("Opened {Asset}", asset);
        return result;
    }

    public T Deserialize<T>(string resourcePath)
    {
        using var stream = Open(resourcePath);
        var result = JsonSerializer.Deserialize<T>(stream);
        ArgumentNullException.ThrowIfNull(result);
        return result;
    }

    public IEnumerable<string> GetAssetPathsByFolder(string resourceFolderPath) => Catalog
        .GetAssetsByFolder(resourceFolderPath)
        .Select(x => x.ResourcePath);
}

file static class UriExtensions
{
    public static Uri ToBaseUri(this string assemblyName) => new($"avares://{assemblyName}");

    public static Uri ToUri(this Asset asset)
    {
        if (!Uri.TryCreate(asset.AssemblyName.ToBaseUri(), asset.ResourcePath, out var uri))
        {
            throw new ArgumentException($"Could not create URI from asset {asset}");
        }

        return uri;
    }
}
