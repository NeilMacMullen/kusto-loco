using System;
using System.Collections.Immutable;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
///     Allows a column to arbitrarily remap rows to those in another column
/// </summary>
[KustoGeneric(Types = "all")]
public class GenericMappedColumn<T> : GenericTypedBaseColumn<T>
{
    private readonly ImmutableArray<int> _lookups;
    public readonly GenericTypedBaseColumn<T> BackingColumn;

    private GenericMappedColumn(ImmutableArray<int> lookups, GenericTypedBaseColumn<T> backing)
    {
        _lookups = lookups;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];

    public override int RowCount => _lookups.Length;


    public static GenericTypedBaseColumn<T> Create(ImmutableArray<int> lookups, BaseColumn rawBacking)
    {
        if (rawBacking is GenericSingleValueColumn<T> single)
            return single.ResizeTo(lookups.Length);
        var backing = (GenericTypedBaseColumn<T>)rawBacking;
        return new GenericMappedColumn<T>(lookups, backing);
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
        return new GenericMappedColumn<T>(slicedData, BackingColumn);
    }


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));
}
