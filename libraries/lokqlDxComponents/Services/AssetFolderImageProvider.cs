using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Svg;
using Avalonia.Threading;
using Intellisense;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Views;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public class AssetFolderImageProvider(
    AppOptions options,
    ILogger<AssetFolderImageProvider> logger,
    IInternalAssetLoader internalAssetLoader
    )
    : IImageProvider
{
    private bool _initialized;
    private Dictionary<IntellisenseHint, Uri> _hintToAssetUriMappings = [];

    public IImage GetImage(IntellisenseHint imageSource)
    {
        if (!_initialized)
        {
            Init();
        }

        return _hintToAssetUriMappings.TryGetValue(imageSource, out var uri)
            ? internalAssetLoader.LoadImage(uri)
            : NullImage.Instance;
    }

    public void Init()
    {
        // reinitialize state
        _initialized = false;
        _hintToAssetUriMappings = [];

        List<Uri> assets;
        try
        {
            assets = internalAssetLoader.GetAssets(options.CompletionIconsUri).ToList();
        }
        catch (Exception e)
        {
            // AssetLoader retrieves assets from compiled binary.
            // If that fails, there is either a fundamental issue at compilation or incorrect URI configured, which we cannot recover from.
            // So gracefully degrade and disable image loading
            logger.LogCritical(e, "Failed to retrieve assets.");
            _hintToAssetUriMappings = [];
            _initialized = true;
            return;
        }

        // only include enum members that have an associated image to reduce unnecessary checks
        _hintToAssetUriMappings = Enum
            .GetValues<IntellisenseHint>()
            .Join(assets,
                x => x.ToString(),
                x => Path.GetFileNameWithoutExtension(x.AbsolutePath),
                KeyValuePair.Create,
                StringComparer.OrdinalIgnoreCase
            )
            .ToDictionary();

        logger.LogInformation("Loaded mappings with {ItemCount} items", _hintToAssetUriMappings.Count);
        _initialized = true;
    }
}

public class InternalAssetLoader(ILogger<InternalAssetLoader> logger) : IInternalAssetLoader
{
    private readonly Dictionary<Uri, IImage> _imageAssetCache = [];

    private static IImage CreateImage(Uri uri, Stream stream)
    {
        if (uri.AbsolutePath.EndsWith(".svg"))
        {
            return new SvgImage { Source = SvgSource.Load(stream) };
        }


        return new Bitmap(stream);
    }

    public IEnumerable<Uri> GetAssets(Uri uri) => AssetLoader.GetAssets(uri, null);

    public IImage LoadImage(Uri uri)
    {
        if (_imageAssetCache.TryGetValue(uri, out var image))
        {
            logger.LogTrace("Found image in cache");
            return image;
        }


        try
        {
            if (!AssetLoader.Exists(uri))
            {
                logger.LogTrace("Asset not found");
                return NullImage.Instance;
            }

            var stream = AssetLoader.Open(uri);
            logger.LogTrace("Loaded asset");
            var res = Dispatcher.UIThread.Invoke(() => CreateImage(uri, stream));
            logger.LogTrace("Created image");
            _imageAssetCache[uri] = res;
            return res;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading image");
            return NullImage.Instance;
        }
    }
}
