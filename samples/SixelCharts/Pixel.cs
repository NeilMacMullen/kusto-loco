public readonly record struct Pixel(byte R, byte G, byte B, byte A)
{
    public static Pixel FromArgb(int A, int R, int G, int B)
    {
        return new Pixel((byte)R, (byte)G, (byte)B, (byte)A);
    }
}