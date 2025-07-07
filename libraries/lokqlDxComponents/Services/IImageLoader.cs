using Avalonia.Media;

namespace lokqlDxComponents.Services;

public interface IImageLoader
{
    IImage LoadImage(Uri uri);
}
