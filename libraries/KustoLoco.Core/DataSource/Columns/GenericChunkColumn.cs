using System;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
///     A ChunkColumn is less sophisticated than a Mapped column - it just allows a contiguous block
///     of the source column to be used
/// </summary>
/// <typeparam name="T"></typeparam>
[KustoGeneric(Types = "all")]
public class GenericChunkColumn<T> : GenericTypedBaseColumn<T>
{
    private readonly int _length;
    private readonly int _offset;
    public readonly GenericTypedBaseColumn<T> BackingColumn;

    private GenericChunkColumn(int offset, int length, GenericTypedBaseColumn<T> backing)
    {
        _offset = offset;
        _length = length;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];

    public override int RowCount => _length;


    public static GenericTypedBaseColumn<T> Create(int offset, int length, BaseColumn rawBacking)
    {
        if (rawBacking is not GenericTypedBaseColumn<T> backing)
        {
            throw new InvalidOperationException("wrong type");
        }
        if (backing is GenericSingleValueColumn<T> single)
            return single.ResizeTo(length);
        //if we're actually slicing the whole column, just return the original backing column
        if (offset==0 && length == backing.RowCount)
            return backing;
        return new GenericChunkColumn<T>(offset, length, backing);
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
