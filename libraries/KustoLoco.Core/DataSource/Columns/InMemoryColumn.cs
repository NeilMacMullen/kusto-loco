using System;

namespace KustoLoco.Core.DataSource.Columns;

public class InMemoryColumn<T> : TypedBaseColumn<T>
{
    private readonly T?[] _data;

    public InMemoryColumn(T?[] data) =>
        _data = data ?? throw new ArgumentNullException(nameof(data));


    public override T? this[int index] => _data[index];

    public override int RowCount => _data.Length;

    public override object? GetRawDataValue(int index) => _data[index];

    public override BaseColumn Slice(int start, int length)
    {
        return ChunkColumn<T>.Create(start, length, this);
    }

    public override void ForEach(Action<object?> action)
    {
        foreach (var item in _data)
        {
            action(item);
        }
    }
}
