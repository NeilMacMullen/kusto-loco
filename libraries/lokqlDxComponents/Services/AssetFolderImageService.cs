using System.Diagnostics;
using Avalonia.Media;
using Intellisense;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Views;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public class AssetFolderImageService : IImageProvider
{
    private readonly Dictionary<IntellisenseHint, Uri> _hintToAssetUriMappings;
    private readonly Dictionary<Uri, IImage> _imageAssetCache = new();
    private readonly ILogger<AssetFolderImageService> _logger;
    private readonly IImageLoader _imageLoader;


    public AssetFolderImageService(
        ImageServiceOptions options,
        ILogger<AssetFolderImageService> logger,
        IImageLoader imageLoader,
        IFileExtensionService fileExtensionService
    )
    {
        _logger = logger;
        _imageLoader = imageLoader;

        _hintToAssetUriMappings = fileExtensionService
            .GetFileExtensions()
            .Concat([IntellisenseHint.Folder, IntellisenseHint.File])
            .Select(CreateAssetEntry)
            .ToDictionary();

        _logger.LogTrace("Created {@HintToAssetMappings}", _hintToAssetUriMappings);
        _logger.LogInformation("Initialized with {AssetCount} items", _hintToAssetUriMappings.Count);

        return;

        KeyValuePair<IntellisenseHint, Uri> CreateAssetEntry(IntellisenseHint hint)
        {
            var fileName = (hint + options.Extension).ToLower();
            var location = new Uri(options.AssetsFolder, fileName);

            return KeyValuePair.Create(hint, location);
        }
    }


    private IImage GetImage(Uri uri)
    {

        using var _ = _logger.BeginScope(new Dictionary<string, object> { [nameof(Uri)] = uri });
        if (_imageAssetCache.TryGetValue(uri, out var image))
        {
            _logger.LogTrace("Found image in cache");
            return image;
        }

        try
        {
            var res = _imageLoader.LoadImage(uri);
            _logger.LogDebug("Loaded image");
            _imageAssetCache[uri] = res;
            return res;
        }
        catch (FileNotFoundException e)
        {
            _logger.LogWarning(e, "Extension type not supported");
            return NullImage.Instance;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading image");
            return NullImage.Instance;
        }
    }

    public IImage GetImage(IntellisenseHint imageSource)
    {

        if (_hintToAssetUriMappings.TryGetValue(imageSource, out var uri))
        {
            return GetImage(uri);
        }

        Debug.Assert(imageSource is IntellisenseHint.None, "Map should already be populated with all possible values.");

        return NullImage.Instance;
    }
}
