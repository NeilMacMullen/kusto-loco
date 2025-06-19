
namespace KustoLoco.Rendering.SixelSupport;

/// <summary>
/// Surfaces a byte array  as a PixelSource
/// </summary>
/// <remarks> Currently used to show ScottPlot bmps but could
/// also be used to render bitmaps from files etc
/// </remarks>
public class ArgbPixelSource : IPixelSource

{
    private readonly int _bytesPerPixel;
    private readonly byte[] _bytes;
    private readonly bool _invert;
    private readonly int _headerSize;
    public int Height { get; }
    public int Width { get; }
    public Pixel GetPixel(int x, int y)
    {
        y =  _invert ?
            y : 
            Height - y - 1; // Invert y coordinate for BMP format
        var offset = (y * Width + x) * _bytesPerPixel + _headerSize;
        return
            _bytesPerPixel == 3
                ? Pixel.FromRgb(_bytes[offset + 2], _bytes[offset + 1], _bytes[offset])
                : Pixel.FromArgb(_bytes[offset + 3], _bytes[offset + 2], _bytes[offset + 1], _bytes[offset]);
    }

    /// <summary>
    /// Reads a 32 bit value from a set of bytes in a BMP file
    /// </summary>
    private static uint ReadUint32(byte[] bytes, int offset)
    {
        return (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
    }

   

    private ArgbPixelSource(byte[] bytes, int width, int height, int headerSize,int bytesPerPixel)
    {
        _bytes = bytes;
        Width = width;
        Height = Math.Abs(height);
        if (height < 0) _invert = true;
        _headerSize = headerSize;
        _bytesPerPixel = bytesPerPixel;
    }

    
    private const int ExpectedHeaderSize=54;
    public static ArgbPixelSource FromScottPlotBmp(byte[] rawData)
    {
        var width = (int)ReadUint32(rawData, 18);
        var rawHeight = (int)ReadUint32(rawData, 22);
        return new ArgbPixelSource(rawData, width, rawHeight, ExpectedHeaderSize,4);

    }
}
