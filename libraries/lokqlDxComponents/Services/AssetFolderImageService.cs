using System.IO.Abstractions;
using Avalonia.Media;
using Intellisense.FileSystem;
using lokqlDxComponents.Configuration;
using lokqlDxComponents.Views;
using Microsoft.Extensions.Logging;
using NotNullStrings;

namespace lokqlDxComponents.Services;


public class AssetFolderImageService(
    ImageServiceOptions options,
    ILogger<AssetFolderImageService> logger,
    IImageLoader imageLoader

)
    : IImageProvider, IImageSourceLocator
{
    private readonly Dictionary<Uri, IImage> _cache = new();
    private const string FolderImage = "folder";
    private const string FileImage = "file";

    public Uri GetImageSource(IFileSystemInfo fileSystemInfo)
    {
        string fileName;

        if (fileSystemInfo is IDirectoryInfo)
        {
            fileName = FolderImage;
        }
        else
        {
            var fsExt = fileSystemInfo.Extension;
            fileName = fsExt.IsBlank() ? FileImage : fsExt;
        }

        fileName = fileName.TrimStart('.') + options.Extension;

        if (!Uri.TryCreate(options.AssetsFolder, fileName, out var source))
        {
            logger.LogError("Failed to create source URI {AssetName}", fileName);
            return UriExtensions.EmptyUri;
        }

        logger.LogTrace("Created {SourceUri}", source);

        return source;
    }


    public IImage GetImage(Uri uri)
    {
        if (uri.IsEmpty())
        {
            logger.LogTrace("Empty URI");
            return NullImage.Instance;
        }

        using var _ = logger.BeginScope(new Dictionary<string, object> { [nameof(Uri)] = uri });
        if (_cache.TryGetValue(uri, out var image))
        {
            logger.LogTrace("Found image in cache");
            return image;
        }

        try
        {
            var res = imageLoader.LoadImage(uri);
            logger.LogDebug("Loaded image");
            _cache[uri] = res;
            return res;
        }
        catch (FileNotFoundException e)
        {
            logger.LogWarning(e, "Extension type not supported");
            return NullImage.Instance;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error loading image");
            return NullImage.Instance;
        }
    }
}
