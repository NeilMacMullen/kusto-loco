using Avalonia.Media;
using Intellisense;

namespace lokqlDxComponents.Services;

public interface IImageProvider
{
    IImage GetImage(IntellisenseHint imageSource);
}
