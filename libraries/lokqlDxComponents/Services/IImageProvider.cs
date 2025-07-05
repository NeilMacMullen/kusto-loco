using Avalonia.Media;

namespace lokqlDxComponents.Services;

public interface IImageProvider
{
    IImage GetImage(string imageSource);
}
