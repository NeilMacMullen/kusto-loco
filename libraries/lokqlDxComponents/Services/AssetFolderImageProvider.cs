using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Svg;
using Avalonia.Threading;
using Intellisense;
using lokqlDxComponents.Views;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public class AssetFolderImageProvider(
    ILogger<AssetFolderImageProvider> logger,
    IAssetService assetService
)
    : IImageProvider
{
    private bool _initialized;
    private Dictionary<IntellisenseHint, string> _hintToAssetPathMappings = [];
    private readonly Dictionary<string, IImage> _imageCache = [];

    public IImage GetImage(IntellisenseHint imageSource)
    {
        if (!_initialized)
        {
            logger.LogInformation("Service has not been initialized yet. Initializing.");
            Init();
        }

        var image = _hintToAssetPathMappings.TryGetValue(imageSource, out var uri)
            ? LoadImage(uri)
            : NullImage.Instance;
        return image;
    }

    public void Init()
    {
        logger.LogInformation("Using {CompletionIconsPath} for assets.", AssetLocations.CompletionIcons);
        // reinitialize state
        _initialized = false;
        _hintToAssetPathMappings = [];

        var assets = assetService.GetAssetPathsByFolder(AssetLocations.CompletionIcons).ToList();

        // only include enum members that have an associated image to reduce unnecessary checks
        _hintToAssetPathMappings = Enum
            .GetValues<IntellisenseHint>()
            .Join(assets,
                x => x.ToString(),
                Path.GetFileNameWithoutExtension,
                KeyValuePair.Create,
                StringComparer.OrdinalIgnoreCase
            )
            .ToDictionary();

        // prepopulate cache
        foreach (var hintToAssetUriMapping in _hintToAssetPathMappings)
        {
            LoadImage(hintToAssetUriMapping.Value);
        }

        logger.LogInformation("Loaded mappings with {ItemCount} items", _hintToAssetPathMappings.Count);
        _initialized = true;
    }

    private static IImage CreateImage(string path, Stream stream)
    {
        if (stream.Length is 0)
        {
            return NullImage.Instance;
        }
        if (path.EndsWith(".svg"))
        {
            return new SvgImage { Source = SvgSource.Load(stream) };
        }


        return new Bitmap(stream);
    }

    private IImage LoadImage(string path)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object> { ["AssetPath"] = path });
        if (_imageCache.TryGetValue(path, out var image))
        {
            logger.LogTrace("Found image in cache");
            return image;
        }

        if (!assetService.Exists(path))
        {
            logger.LogTrace("Asset does not exist");
            return NullImage.Instance;
        }

        try
        {
            var stream = assetService.Open(path);
            logger.LogTrace("Loaded asset");
            var res = Dispatcher.UIThread.Invoke(() => CreateImage(path, stream));
            logger.LogTrace("Created image");
            _imageCache[path] = res;
            return res;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading image");
            return NullImage.Instance;
        }
    }
}