using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Clowd.Clipboard;

namespace LokqlDx.Views;

public static class ControlRenderer
{
    public static void SaveControlToClipboard(Control control)
    {
        if (!OperatingSystem.IsWindows()) return;
        try
        {
            var size = control.Bounds.Size;
            control.Measure(size);
            control.Arrange(new Rect(size));
            // Create a RenderTargetBitmap with DPI awareness
            var pixelSize = new PixelSize((int)size.Width, (int)size.Height);
            var dpi = new Vector(96, 96); // Adjust if you're targeting high-DPI displays

            using var rtb = new RenderTargetBitmap(pixelSize, dpi);
            rtb.Render(control);
            // Encode to PNG in memory
            using var memoryStream = new MemoryStream();
            rtb.Save(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            var bitmap = new Bitmap(memoryStream);
            ClipboardAvalonia.SetImage(bitmap);
        }
        catch
        {
        }
    }
}
