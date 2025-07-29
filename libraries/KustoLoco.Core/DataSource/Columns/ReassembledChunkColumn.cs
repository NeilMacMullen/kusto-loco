using System;
using System.Collections.Generic;
using System.Linq;

namespace KustoLoco.Core.DataSource.Columns;
#if false
/// <summary>
///     Represents a column formed of one or more sections which are processed in order
/// </summary>
/// <typeparam name="T"></typeparam>
public class ReassembledChunkColumn<T> : TypedBaseColumn<T>
{
    private readonly int _Length;

    private readonly Section[] BackingColumns;

    private Section _lastHitSection;

    private ReassembledChunkColumn(IEnumerable<TypedBaseColumn<T>> backing)
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
        _lastHitSection = sections.First();
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
        //most accesses are sequential so we can avoid a lot of scanning by just
        //assuming the last section we used will serve the current request
        if (!IndexInSection(_lastHitSection, index))
        {
            _lastHitSection = Section.Empty;
            foreach (var section in BackingColumns)
                if (IndexInSection(section, index))
                {
                    _lastHitSection = section;
                    break;
                }
        }

        if (_lastHitSection == Section.Empty)
            throw new InvalidOperationException(
                $"Requested an index {index} which is greater than rowcount {RowCount} with {BackingColumns.Length} backing columns");

        return (index - _lastHitSection.Offset, _lastHitSection.BackingColumn);
    }

    private static bool IndexInSection(Section s, int i) => i >= s.Offset && i < s.Offset + s.Length;

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++) action(this[i]);
    }

    public override BaseColumn Slice(int start, int length)
    {
        var slicedColumns = new List<TypedBaseColumn<T>>();
        foreach (var s in BackingColumns)
            if (IndexInSection(s, start))
            {
                var top = start + length;
                if (top <= s.Offset + s.Length)
                {
                    var col = s.BackingColumn.Slice(start - s.Offset, length)
                        as TypedBaseColumn<T>;
                    slicedColumns.Add(col!);
                    break;
                }
                else
                {
                    var sliceLength = s.Offset + s.Length - start;
                    var col = s.BackingColumn.Slice(start - s.Offset, sliceLength)
                        as TypedBaseColumn<T>;
                    slicedColumns.Add(col!);
                    length -= sliceLength;
                    start = s.Offset + s.Length;
                }
            }

        return new ReassembledChunkColumn<T>(slicedColumns);
    }

    public static TypedBaseColumn<T> Create(IReadOnlyCollection<TypedBaseColumn<T>> backing) =>
        //If there's only one column, just return it since no reassembly is needed
        backing.Count == 1
            ? backing.First()
            : new ReassembledChunkColumn<T>(backing);

    public override object? GetRawDataValue(int index)
    {
        var (i, chunk) = IndirectIndex(index);
        return chunk[i];
    }

    private readonly record struct Section(int Offset, int Length, TypedBaseColumn<T> BackingColumn)
    {
        public static readonly Section Empty = new(0, 0, ColumnFactory.Create(Array.Empty<T>()));
    }
}

#endif
