using Avalonia;
using Avalonia.Media;

namespace lokqlDxComponents.Views;

public class NullImage : IImage
{
    private NullImage()
    {

    }
    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
    }

    public Size Size { get; set; } = new();

    public static readonly IImage Instance = new NullImage();
}