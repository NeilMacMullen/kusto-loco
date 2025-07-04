using Avalonia;
using Avalonia.Media;

namespace lokqlDxComponents.Models;

public class NullImage : IImage
{
    public void Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
    }

    public Size Size { get; set; } = new();

    public static readonly IImage Instance = new NullImage();
}
