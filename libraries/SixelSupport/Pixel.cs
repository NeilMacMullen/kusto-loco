namespace KustoLoco.Rendering.SixelSupport;


/// <summary>
/// Simple Reimplementation of ARGB pixel for platform independence
/// </summary>
public readonly record struct Pixel(byte R, byte G, byte B, byte A)
{
    public static Pixel FromRgb(int r, int g, int b)
        => FromArgb(255,r,g,b);

    public static Pixel FromArgb(int a, int r, int g, int b)
    {
        return new Pixel((byte)r, (byte)g, (byte)b, (byte)a);
    }
}
