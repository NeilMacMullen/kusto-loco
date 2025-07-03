using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Svg.Skia;

namespace lokqlDxComponents.Services;

public interface IImageService
{
    IImage GetImage(string filePath);
}

public class ImageService : IImageService
{
    private static IImageService? _instance;
    public static IImageService Instance => _instance ??= new ImageService();

    private const string UriDirectory = "avares://lokqlDx/Assets/FileIcons/";

    private readonly Dictionary<string, IImage> _images = new();

    public IImage GetImage(string filePath)
    {
        var ext = filePath.TrimStart('.');
        if (ext is "")
        {
            ext = "folder";
        }

        if (_images.TryGetValue(ext, out var image))
        {
            return image;
        }

        st1:
        var uri = new Uri(Path.Combine(UriDirectory, ext + ".svg"));
        if (!AssetLoader.Exists(uri))
        {
            ext = "file";
            goto st1;
        }
        var res = LoadImage(uri);
        _images[ext] = res;
        return res;
    }

    private IImage LoadImage(Uri uri) => new SvgImage
    {
        Source = new SvgSource(uri)
        {
            Path = uri.AbsolutePath
        }
    };
}
