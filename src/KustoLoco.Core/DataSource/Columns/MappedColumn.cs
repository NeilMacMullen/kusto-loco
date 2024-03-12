using System;
using System.Collections.Immutable;

namespace KustoLoco.Core;

/// <summary>
///     Allows a column to arbitrarily remap rows to those in another column
/// </summary>
public class MappedColumn<T> : TypedBaseColumn<T>
{
    private readonly ImmutableArray<int> _lookups;
    public readonly TypedBaseColumn<T> BackingColumn;

    private MappedColumn(ImmutableArray<int> lookups, TypedBaseColumn<T> backing)
    {
        _lookups = lookups;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];

    public override int RowCount => _lookups.Length;


    public static TypedBaseColumn<T> Create(ImmutableArray<int> lookups, TypedBaseColumn<T> backing)
    {
        if (backing is SingleValueColumn<T> single)
            return single.ResizeTo(lookups.Length);
        return new MappedColumn<T>(lookups, backing);
    }

    public int IndirectIndex(int index) => _lookups[index];

    public override void ForEach(Action<object?> action)
    {
        foreach(var i in _lookups)
        {
            action(BackingColumn[i]);
        }
    }

    public override BaseColumn Slice(int start, int length)
    {
        var slicedData = _lookups.Slice(start, length);
        return new MappedColumn<T>(slicedData, BackingColumn);
    }


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));
}