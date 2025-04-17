internal class QuantizedPixelSource : IPixelSource
{
    private readonly Pixel[] _pixels;

    public QuantizedPixelSource(int width, int height)
    {
        Height = height;
        Width = width;
        _pixels = Enumerable.Range(0, width * height)
            .Select(_ => new Pixel(0, 0, 0, 0))
            .ToArray();
    }

    public int Height { get; private set; }
    public int Width { get; private set; }
    public Pixel GetPixel(int x, int y)
    {
        return _pixels[x + y * Width];
    }

    public void SetPixel(int x, int y, Pixel p)
    {
        _pixels[x + y * Width] = p;
    }
}