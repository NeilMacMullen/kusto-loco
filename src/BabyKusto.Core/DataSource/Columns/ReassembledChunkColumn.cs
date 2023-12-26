using System;
using System.Collections.Generic;

namespace BabyKusto.Core;

/// <summary>
///     Represents a column formed of one or more sections which are processed in order
/// </summary>
/// <typeparam name="T"></typeparam>
public class ReassembledChunkColumn<T> : TypedBaseColumn<T>
{
    private readonly int _Length;

    private readonly Section[] BackingColumns;

    public ReassembledChunkColumn(IEnumerable<TypedBaseColumn<T>> backing)
    {
        var sections = new List<Section>();
        var offset = 0;
        foreach (var b in backing)
        {
            var s = new Section(offset, b.RowCount, b);
            offset += b.RowCount;
            sections.Add(s);
        }

        BackingColumns = sections.ToArray();
        _Length = offset;
    }

    public override T? this[int index]
    {
        get
        {
            var (i, chunk) = IndirectIndex(index);
            return chunk[i];
        }
    }

    public override int RowCount => _Length;

    private (int, TypedBaseColumn<T>) IndirectIndex(int index)
    {
        foreach (var s in BackingColumns)
        {
            if (index >= s.Offset && index < (s.Offset + s.Length))
                return (index - s.Offset, s.BackingColumn);
        }

        throw new InvalidOperationException(
            $"Requested an index {index} which is greater than rowcount {RowCount} with {BackingColumns.Length} backing columns");
    }


    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
        {
            action(this[i]);
        }
    }

    public override BaseColumn Slice(int start, int length) =>
        throw new NotImplementedException("Can't yet slice reassembled chunks");


    public override object? GetRawDataValue(int index)
    {
        var (i, chunk) = IndirectIndex(index);
        return chunk[i];
    }

    private readonly record struct Section(int Offset, int Length, TypedBaseColumn<T> BackingColumn);
}