using Avalonia.Media;
using Avalonia.Svg;


namespace lokqlDxComponents.Services;

public class SvgLoader : IImageLoader
{
    public IImage LoadImage(Uri uri) => new SvgImage
    {
        Source = SvgSource.Load(uri.ToString(), null)
    };
}
