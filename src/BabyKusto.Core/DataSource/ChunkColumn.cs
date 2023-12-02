using System;
using System.ComponentModel.DataAnnotations;
using System.Security.AccessControl;

namespace BabyKusto.Core;

/// <summary>
/// A ChunkColumn is less sophisticated than a Mapped column - it just allows a contiguous block
/// of the source column to be used
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChunkColumn<T> : Column<T>
{
    private readonly int _offset;
    private readonly int _length;
    public readonly Column<T> BackingColumn;

    private ChunkColumn(int offset, int length , Column<T> backing)
        : base(backing.Type, Array.Empty<T>())
    {
        _offset = offset;
        _length = length;
        BackingColumn = backing;
    }
    public static Column<T> Create(int offset,int length, Column<T> backing)
    {
        if (backing is SingleValueColumn<T> single)
            return single.ResizeTo(length);
        return new ChunkColumn<T>(offset,length, backing);
    }
    public override T? this[int index] => BackingColumn[IndirectIndex(index)];
    public override int RowCount => _length;

    public int IndirectIndex(int index) => _offset+index;

    public override void ForEach(Action<object?> action)
    {
        for (var i =0; i < RowCount; i++)
        {
            action(this[i]);
        }
    }

    public override Column Slice(int start, int length)
    {
        return Create(_offset+start,length,BackingColumn );
    }


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));
}