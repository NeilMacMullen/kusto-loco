using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "all")]
public class GenericInMemoryColumn<T> : GenericTypedBaseColumn<T>
{
    private readonly NullableSet<T> _nullableSet;

    public GenericInMemoryColumn(NullableSet<T> data)
    {
        _nullableSet = data;
        if (typeof(T) == typeof(JsonArray))
        {
            throw new InvalidOperationException(
                "InMemoryColumn cannot be used for JsonArray data. Use JsonNode instead");
        }
    }

    public GenericInMemoryColumn(object?[] data)
    {
        _nullableSet =   NullableSet<T>.FromObjectsOfCorrectType(data);
        if (typeof(T)== typeof(JsonArray))
        {
            throw new InvalidOperationException(
                "InMemoryColumn cannot be used for JsonArray data. Use JsonNode instead");
        }
    }

    public override T? GetNullableT(int index) => this[index];

    public override T? this[int index] => (T?) _nullableSet.NullableT(index) ;

    public override int RowCount => _nullableSet.Length;

    public override object? GetRawDataValue(int index) => _nullableSet.NullableObject(index);

    public override BaseColumn Slice(int start, int length)
    {
        return GenericChunkColumn<T>.Create(start, length, this);
    }

    public override void ForEach(Action<object?> action)
    {
        throw new NotImplementedException();
    }
}
