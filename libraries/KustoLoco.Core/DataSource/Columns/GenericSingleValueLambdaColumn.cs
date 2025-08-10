using System;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "all")]
public class GenericSingleValueLambdaColumn<T> : GenericTypedBaseColumn<T>
{
    private readonly Func<T?> _dataFetcher; //GENERIC INLINE
    private readonly int _length;

    public GenericSingleValueLambdaColumn(
        Func<T?> dataFetcher,//GENERIC INLINE
        int rowCount)
    {
        _dataFetcher = dataFetcher;
        _length = rowCount;
        hints = ColumnHints.HoldsSingleValue;
    }


    public override T? GetNullableT(int index) => _dataFetcher();

    public override T? this[int index] => _dataFetcher();

    public override int RowCount => _length;
    public override object? GetRawDataValue(int index) => this[index];

    public override BaseColumn Slice(int start, int length) => new GenericSingleValueLambdaColumn<T>(_dataFetcher, length);

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
            action(GetRawDataValue(i));
    }
}
