using System;
using System.Collections.Immutable;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
/// Provides an efficient way to access an array of existing values
/// </summary>
/// <remarks>
/// When we're provided with an immutable array of records, we can simply share the array and provide a lambda per
/// column to access the data.
/// </remarks>
public class OldLambdaWrappedColumn<TRow, T> : OldTypedBaseColumn<T>
{
    private readonly Func<TRow, T> _dataFetcher;

    private readonly ImmutableArray<TRow> _rows;

    public OldLambdaWrappedColumn(ImmutableArray<TRow> rows, Func<TRow, T> dataFetcher)
    {
        _dataFetcher = dataFetcher;
        _rows = rows;
    }

    public override T this[int index] => _dataFetcher(_rows[index]);

    public override int RowCount => _rows.Length;

    public override object? GetRawDataValue(int index) => this[index];

    public override BaseColumn Slice(int start, int length)
        => new OldLambdaWrappedColumn<TRow, T>(_rows.Slice(start, length), _dataFetcher);

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < RowCount; i++)
            action(GetRawDataValue(i));
    }
}
