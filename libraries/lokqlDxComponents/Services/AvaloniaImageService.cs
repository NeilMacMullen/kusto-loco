using System.IO.Abstractions;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;
using Intellisense.FileSystem;
using lokqlDxComponents.Models;
using Microsoft.Extensions.Logging;

namespace lokqlDxComponents.Services;

public class AvaloniaImageService(ILogger<IImageProvider> logger) : IImageProvider, IImageSourceLocator
{
    private const string ImageAssetFolder = "avares://lokqlDx/Assets/FileIcons/";
    private readonly Dictionary<string, IImage> _cache = new();

    public string GetImageSource(IFileSystemInfo fileSystemInfo)
    {
        var fileName = fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory)
            ? "folder"
            : fileSystemInfo.Extension;

        fileName = fileName.TrimStart('.') + ".svg";

        var source = Path.Combine(ImageAssetFolder, fileName);

        logger.LogTrace("Retrieved {Source} for {Path}. {FileName}", source, fileSystemInfo, fileName);
        return source;
    }

    public IImage GetImage(string imageSource)
    {
        if (!Uri.TryCreate(imageSource, UriKind.Absolute, out var uri))
        {
            return NullImage.Instance;
        }

        return GetImage(uri);
    }

    private IImage GetImage(Uri uri)
    {
        var key = uri.AbsolutePath;
        if (_cache.TryGetValue(key, out var image))
        {
            return image;
        }

        if (!AssetLoader.Exists(uri))
        {
            _cache[key] = NullImage.Instance;
            return _cache[key];
        }

        var res = LoadImage(uri);
        logger.LogDebug("Loading image {uri}", uri);
        _cache[key] = res;
        return res;
    }

    private IImage LoadImage(Uri uri) => new SvgImage
    {
        Source = SvgSource.LoadFromStream(AssetLoader.Open(uri))
    };
}
