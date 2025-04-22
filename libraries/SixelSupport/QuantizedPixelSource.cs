namespace KustoLoco.Rendering.SixelSupport;

/// <summary>
/// a colour-quantized image
/// </summary>
/// <remarks> Sixels only support 256 colours so we need to ensure
/// we've quantized the any images we try to use
/// </remarks>
public class QuantizedPixelSource :IPixelSource{
    private readonly IPixelSource _source;
    private readonly Octree.PaletteQuantizer _quantizer;

    const int MaxColourCount =254;
    public QuantizedPixelSource(IPixelSource bitmap)
    {
        _source = bitmap;
        _quantizer = new Octree.PaletteQuantizer();
        for (var x = 0; x < bitmap.Width; x++)
        for (var y = 0; y < bitmap.Height; y++)
        {
            var colour = bitmap.GetPixel(x, y);
            _quantizer.AddColour(colour);
        }

        _quantizer.Quantize(MaxColourCount);
      
    }

    public int Height => _source.Height;
    public int Width => _source.Width;
    public Pixel GetPixel(int x, int y)
    {
        var raw = _source.GetPixel(x, y);
        return _quantizer.GetQuantizedColour(raw);

    }
}

