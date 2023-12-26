using System;

namespace BabyKusto.Core;

/// <summary>
///     A ChunkColumn is less sophisticated than a Mapped column - it just allows a contiguous block
///     of the source column to be used
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChunkColumn<T> : TypedBaseColumn<T>
{
    private readonly int _length;
    private readonly int _offset;
    public readonly TypedBaseColumn<T> BackingColumn;

    private ChunkColumn(int offset, int length, TypedBaseColumn<T> backing)
    {
        _offset = offset;
        _length = length;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];
    public override int RowCount => _length;

    public static TypedBaseColumn<T> Create(int offset, int length, TypedBaseColumn<T> backing)
    {
        if (backing is SingleValueColumn<T> single)
            return single.ResizeTo(length);
        return new ChunkColumn<T>(offset, length, backing);
    }

    public int IndirectIndex(int index) => _offset + index;

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
        {
            action(this[i]);
        }
    }

    public override BaseColumn Slice(int start, int length) => Create(_offset + start, length, BackingColumn);


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));
}