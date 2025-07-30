using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.DataSource.Columns;

public class OldInMemoryColumn<T> : OldTypedBaseColumn<T>
{
    private readonly INullableSet _nullableSet;

    public OldInMemoryColumn(INullableSet data)
    {
        _nullableSet = data;
        if (typeof(T) == typeof(JsonArray))
        {
            throw new InvalidOperationException(
                "InMemoryColumn cannot be used for JsonArray data. Use JsonNode instead");
        }
    }

    public OldInMemoryColumn(object?[] data)
    {
        _nullableSet = NullableSetLocator.GetNullableForType(typeof(T), data);
        if (typeof(T)== typeof(JsonArray))
        {
            throw new InvalidOperationException(
                "InMemoryColumn cannot be used for JsonArray data. Use JsonNode instead");
        }
    }

    public override T? this[int index] => (T?) _nullableSet.NullableValue(index) ;

    public override int RowCount => _nullableSet.Length;

    public override object? GetRawDataValue(int index) => _nullableSet.NullableValue(index);

    public override BaseColumn Slice(int start, int length)
    {
        return OldChunkColumn<T>.Create(start, length, this);
    }

    public override void ForEach(Action<object?> action)
    {
        throw new NotImplementedException();
    }
}
