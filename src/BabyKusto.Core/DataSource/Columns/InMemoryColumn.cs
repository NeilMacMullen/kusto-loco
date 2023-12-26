using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public class InMemoryColumn<T> : TypedBaseColumn<T>
{
    private readonly T?[] _data;

    public InMemoryColumn(TypeSymbol type, T?[] data)
        : base(type) =>
        _data = data ?? throw new ArgumentNullException(nameof(data));


    public override int RowCount => _data.Length;

    public override T? this[int index] => _data[index];


    public override object? GetRawDataValue(int index) => _data[index];

    public override BaseColumn Slice(int start, int length)
    {
        var slicedData = new T[length];
        //TODO - we really ought to be able to get away with copying
        //the data here
        Array.Copy(_data, start, slicedData, 0, length);
        return ColumnFactory.Create(Type, slicedData);
    }

    public override void ForEach(Action<object?> action)
    {
        foreach (var item in _data)
        {
            action(item);
        }
    }
}