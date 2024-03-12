using System;
using KustoLoco.Core.Util;

namespace KustoLoco.Core;

public class LambdaColumn<T> : TypedBaseColumn<T>
{
    private readonly Func<int, T?> _dataFetcher;
    private readonly int _length;

    public LambdaColumn(Func<int, T?> dataFetcher, int rowCount)
    {
        _dataFetcher = dataFetcher;
        _length = rowCount;
    }


    public override T? this[int index] => _dataFetcher(index);

    public override int RowCount => _length;

    public override object? GetRawDataValue(int index) => this[index];

    public override BaseColumn Slice(int start, int length)
    {
        var builder = ColumnHelpers.CreateBuilder(Type);
        for (var i = 0; i < length; i++)
        {
            builder.Add(GetRawDataValue(i + start));
        }

        return builder.ToColumn();
    }

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
            action(GetRawDataValue(i));
    }
}