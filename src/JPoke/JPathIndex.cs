namespace JPoke;

public readonly record struct JPathIndex(JPathIndexType IndexType, int RawValue)
{
    public static readonly JPathIndex None = new(JPathIndexType.NotIndex, 0);

    public static JPathIndex CreateAbsolute(int value) => new(JPathIndexType.Absolute, value);

    public static JPathIndex CreateRelative(int value) => new(JPathIndexType.Relative, value);

    public int EffectiveIndex(int arrCount)
    {
        if (IndexType == JPathIndexType.Absolute)
            return RawValue;
        if (IndexType == JPathIndexType.Relative)
            return arrCount + RawValue;
        throw new InvalidOperationException();
    }
}