using System;
using System.Collections.Immutable;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
///     Allows a column to arbitrarily remap rows to those in another column
/// </summary>
public class OldMappedColumn<T> : OldTypedBaseColumn<T>
{
    private readonly ImmutableArray<int> _lookups;
    public readonly OldTypedBaseColumn<T> BackingColumn;

    private OldMappedColumn(ImmutableArray<int> lookups, OldTypedBaseColumn<T> backing)
    {
        _lookups = lookups;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];

    public override int RowCount => _lookups.Length;


    public static OldTypedBaseColumn<T> Create(ImmutableArray<int> lookups, OldTypedBaseColumn<T> backing)
    {
        if (backing is OldSingleValueColumn<T> single)
            return single.ResizeTo(lookups.Length);
        return new OldMappedColumn<T>(lookups, backing);
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
        return new OldMappedColumn<T>(slicedData, BackingColumn);
    }


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));
}
