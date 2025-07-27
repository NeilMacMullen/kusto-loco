using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.DataSource.Columns;

public class InMemoryColumn<T> : TypedBaseColumn<T>
{
    private readonly IMaybeNullable _maybeNullable;

    public InMemoryColumn(object?[] data)
    {
        _maybeNullable = MaybeLocator.GetNullableForType(typeof(T), data);
        if (typeof(T)== typeof(JsonArray))
        {
            throw new InvalidOperationException(
                "InMemoryColumn cannot be used for JsonArray data. Use JsonNode instead");
        }
    }

    public override T? this[int index] => (T?) _maybeNullable.NullableValue(index) ;

    public override int RowCount => _maybeNullable.Length;

    public override object? GetRawDataValue(int index) => _maybeNullable.NullableValue(index);

    public override BaseColumn Slice(int start, int length)
    {
        return ChunkColumn<T>.Create(start, length, this);
    }

    public override void ForEach(Action<object?> action)
    {
        throw new NotImplementedException();
    }
}
