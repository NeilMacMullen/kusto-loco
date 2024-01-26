using System;
using System.Collections.Immutable;

namespace BabyKusto.Core;

public class LambdaWrappedColumn<TRow, T> : TypedBaseColumn<T>
{
    private readonly Func<TRow, T> _dataFetcher;

    private readonly ImmutableArray<TRow> _rows;

    public LambdaWrappedColumn(ImmutableArray<TRow> rows, Func<TRow, T> dataFetcher)
    {
        _dataFetcher = dataFetcher;
        _rows = rows;
    }

    public override T this[int index] => _dataFetcher(_rows[index]);

    public override int RowCount => _rows.Length;

    public override object? GetRawDataValue(int index) => this[index];

    public override BaseColumn Slice(int start, int length)
        => new LambdaWrappedColumn<TRow, T>(_rows.Slice(start, length), _dataFetcher);

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
            action(GetRawDataValue(i));
    }
}
