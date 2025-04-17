public interface IPixelSource
{
    int Height { get; }
    int Width { get; }
    Pixel GetPixel(int x, int y);

    IEnumerable<Pixel> GetPixels()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
                yield return GetPixel(x, y);
        }
    }
}