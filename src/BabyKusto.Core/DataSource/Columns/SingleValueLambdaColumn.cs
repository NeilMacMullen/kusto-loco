using System;

namespace BabyKusto.Core;

public class SingleValueLambdaColumn<T> : TypedBaseColumn<T>
{
    private readonly Func<T?> _dataFetcher;
    private readonly int _length;

    public SingleValueLambdaColumn(Func<T?> dataFetcher, int rowCount)
    {
        _dataFetcher = dataFetcher;
        _length = rowCount;
        hints = ColumnHints.HoldsSingleValue;
    }

    public override int RowCount => _length;

    public override T? this[int index] => _dataFetcher();


    public override object? GetRawDataValue(int index) => this[index];

    public override BaseColumn Slice(int start, int length) => new SingleValueLambdaColumn<T>(_dataFetcher, length);

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
            action(GetRawDataValue(i));
    }
}