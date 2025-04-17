internal class ImageSource : IPixelSource

{
    private readonly byte[] _bytes;
    private readonly bool _invert;
    public int Height { get; }
    public int Width { get; }
    public Pixel GetPixel(int x, int y)
    {
        y =  _invert ?
            y : 
            Height - y - 1; // Invert y coordinate for BMP format
        var offset = (y * Width + x) * BytesPerPixel + 54; // 54 is the header size
        return new Pixel(_bytes[offset + 2], _bytes[offset + 1], _bytes[offset], 255);
    }

    public static ImageSource FromFile(string bmpPath)
    {
        Console.WriteLine($"reading {bmpPath}");
        var bytes = File.ReadAllBytes(bmpPath);
        return FromBytes(bytes);
       
    }
    private static uint ReadUint32(byte[] bytes, int offset)
    {
        return (uint)(bytes[offset] | (bytes[offset + 1] << 8) | (bytes[offset + 2] << 16) | (bytes[offset + 3] << 24));
    }

    public static ImageSource FromBytes(byte[] bytes)
    {
        return new ImageSource(bytes);
    }
    public ImageSource(byte[] bytes)
    {
        _bytes = bytes;
        Width = (int)ReadUint32(_bytes, 18);
        var rawHeight = (int)ReadUint32(_bytes, 22);
        Height = Math.Abs(rawHeight);
        if (rawHeight < 0) _invert = true;
            Console.WriteLine($"bitmap size {Width} x {Height}");
        var expectedsize = Width * Height * BytesPerPixel;
        Console.WriteLine($"expected {expectedsize} got {_bytes.Length} Diff {_bytes.Length - expectedsize}");
     
    }

    private const int BytesPerPixel = 4;
}
