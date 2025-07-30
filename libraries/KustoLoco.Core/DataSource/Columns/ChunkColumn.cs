using System;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
///     A ChunkColumn is less sophisticated than a Mapped column - it just allows a contiguous block
///     of the source column to be used
/// </summary>
/// <typeparam name="T"></typeparam>
public class OldChunkColumn<T> : OldTypedBaseColumn<T>
{
    private readonly int _length;
    private readonly int _offset;
    public readonly OldTypedBaseColumn<T> BackingColumn;

    private OldChunkColumn(int offset, int length, OldTypedBaseColumn<T> backing)
    {
        _offset = offset;
        _length = length;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];

    public override int RowCount => _length;


    public static OldTypedBaseColumn<T> Create(int offset, int length, OldTypedBaseColumn<T> backing)
    {
        if (backing is OldSingleValueColumn<T> single)
            return single.ResizeTo(length);
        //if we're actually slicing the whole column, just return the original backing column
        if (offset==0 && length == backing.RowCount)
            return backing;
        return new OldChunkColumn<T>(offset, length, backing);
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
