using System.Reflection;
using Intellisense.Configuration;
using Jab;
using lokqlDxComponents.Exceptions;
using lokqlDxComponents.Models;
using lokqlDxComponents.Services;
using lokqlDxComponents.Services.Assets;
using Microsoft.Extensions.Logging;


namespace lokqlDxComponents.Configuration;

[ServiceProviderModule]
[Import<IIntellisenseModule>]
[Transient<IntellisenseClientAdapter>]
[Singleton<IImageProvider>(Factory = nameof(GetImageProvider))]
[Singleton<AssetFolderImageProvider>]
[Singleton<IAssetService, AssetService>]
[Singleton<IInternalAssetLoader, InternalAssetLoader>]
[Singleton<AssetCatalog>(Factory = nameof(GetAssetCatalog))]
public interface IAutocompletionModule
{

    public static IImageProvider GetImageProvider(AssetFolderImageProvider provider) => provider;

    public static AssetCatalog GetAssetCatalog(ILogger<AssetCatalog> logger, IInternalAssetLoader assetLoader)
    {
        // AssetLoader retrieves assets from compiled binary.
        // If that fails, there is either a fundamental issue at compilation or incorrect URI configured, which we cannot recover from.


        var assemblies = GetTopologicallySortedAssemblyNames().ToList();

        logger.LogDebug("Retrieved {@Assemblies} for asset catalog construction.", assemblies);


        var assets = assemblies
            .Select(x => new Uri($"avares://{x}"))
            .SelectMany(assetLoader.GetAssets)
            .Select(Asset.Create)
            // AssetLoader returns URIs of other assets other than our own which are located under the root folder.
            // These are of no concern to us at this time so we filter for those that are in the folder we designate for holding assets.
            .Where(x => PathComparer.Instance.Equals(x.BaseDirectory, AssetLocations.Assets));




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

    private static IEnumerable<string> GetTopologicallySortedAssemblyNames()
    {
        var thisAsm = Assembly.GetExecutingAssembly().GetName().Name!;
        return AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(asm => asm.GetReferencedAssemblies().Any(refAsm => refAsm.Name == thisAsm))
            .Select(asm => asm.GetName().Name)
            .OfType<string>()
            .Append(thisAsm);
    }
};
