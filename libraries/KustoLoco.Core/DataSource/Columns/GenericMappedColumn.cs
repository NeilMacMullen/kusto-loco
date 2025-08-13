using System;
using System.Collections.Immutable;
using System.Linq;
using NLog;

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

    
    public override T? this[int index]
    {
       
        
        get {
            var mappedIndex =IndirectIndex(index);
            if (mappedIndex <0)
                return
#if TYPE_STRING
                string.Empty;
#else
                  default(T?);
#endif
            return BackingColumn[mappedIndex];
        }
    }

    public override int RowCount => _lookups.Length;


    public static GenericTypedBaseColumn<T> Create(ImmutableArray<int> lookups, BaseColumn rawBacking)
    {
        //indices of -1 can't be handled by generic single column because we need to know which index
        //the came from
        var anyNegative = lookups.Any(v => v < 0);
        if (!anyNegative)
        {
            if (rawBacking is GenericSingleValueColumn<T> single)
                return single.ResizeTo(lookups.Length);
        }

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
        if (slicedData.Length == 0)
            throw new InvalidOperationException();
        return new GenericMappedColumn<T>(slicedData, BackingColumn);
    }


    public override object? GetRawDataValue(int index)
    {
        var mappedIndex = IndirectIndex(index);
        if (mappedIndex < 0)
            return
#if TYPE_STRING
                string.Empty;
#else
                null;
        #endif
        return this[index];
    }
}
