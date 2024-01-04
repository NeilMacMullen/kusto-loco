namespace JPoke;

public readonly record struct JPathElement(string Name, JPathIndex Index)
{
    public static readonly JPathElement Empty = new(string.Empty, JPathIndex.None);
    public bool IsIndex => Index.IndexType != JPathIndexType.NotIndex;
}