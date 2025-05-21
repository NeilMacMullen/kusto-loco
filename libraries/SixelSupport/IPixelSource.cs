namespace KustoLoco.Rendering.SixelSupport;

/// <summary>
/// Interface intended to abstract away image implementation
/// </summary>
/// <remarks>We wish to avoid dependng on System.Drawing etc
/// </remarks>
public interface IPixelSource
{
    /// <summary>
    /// The height of the image in pixels
    /// </summary>
    int Height { get; }
    /// <summary>
    /// The width of the image in pixels
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Obtain a pixel at coordinates x,y
    /// </summary>
    Pixel GetPixel(int x, int y);

    /// <summary>
    /// Enumerates over all pixels column by column
    /// </summary>
    IEnumerable<Pixel> EnumeratePixelsByColumn()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
                yield return GetPixel(x, y);
        }
    }
}
