using System;
using System.Collections.Immutable;

namespace BabyKusto.Core;

/// <summary>
///     Allows a column to arbitrarily remap rows to those in another column
/// </summary>
public class MappedColumn<T> : Column<T>
{
    private readonly ImmutableArray<int> _lookups;
    public readonly Column<T> BackingColumn;

    private MappedColumn(ImmutableArray<int> lookups, Column<T> backing)
        : base(backing.Type, Array.Empty<T>())
    {
        _lookups = lookups;
        BackingColumn = backing;
    }

    public override T? this[int index] => BackingColumn[IndirectIndex(index)];
    public override int RowCount => _lookups.Length;

    public static Column<T> Create(ImmutableArray<int> lookups, Column<T> backing)
    {
        if (backing is SingleValueColumn<T> single)
            return single.ResizeTo(lookups.Length);
        return new MappedColumn<T>(lookups, backing);
    }

    public int IndirectIndex(int index) => _lookups[index];

    public override void ForEach(Action<object?> action)
    {
        foreach (var i in _lookups)
        {
            action(this[i]);
        }
    }

    public override Column Slice(int start, int length)
    {
        var slicedData = _lookups.Slice(start, length);
        return new MappedColumn<T>(slicedData, BackingColumn);
    }


    public override object? GetRawDataValue(int index) => BackingColumn.GetRawDataValue(IndirectIndex(index));
}