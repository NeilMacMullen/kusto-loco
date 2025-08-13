using System;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
///     A column that holds a single value
/// </summary>
/// <remarks>
///     This is a special case of a column that holds a single value. This is actually quite common when we perform
///     summarizations
///     or filtering operations.  Single-value columns can also efficiently represent "index" columns since a single filter
///     comparison
///     can select a large number of rows.
/// </remarks>
[KustoGeneric(Types = "all")]
public class GenericSingleValueColumn<T> : GenericTypedBaseColumn<T>
{
    private readonly int _rowCount;

    private readonly T? Value;

    public GenericSingleValueColumn(object? value, int nominalRowCount)
    {
        _rowCount = nominalRowCount;
        Value = (T?)value;
        hints = ColumnHints.HoldsSingleValue;
    }

    public override T? this[int index]
    {
        get
        {
            if (index < 0)
                return
#if TYPE_STRING
                   string.Empty;
#else
                    default;
#endif
            return Value;
        }
    }

    public override int RowCount => _rowCount;

    public override void ForEach(Action<object?> action)
    {
        for (var i = 0; i < _rowCount; i++) action(Value);
    }

    public override BaseColumn Slice(int start, int length) => new GenericSingleValueColumn<T>(Value, length);

    public override object? GetRawDataValue(int index) => this[index];

    public GenericTypedBaseColumn<T> ResizeTo(int lookupsLength) =>
        new GenericSingleValueColumn<T>(Value, lookupsLength);
}
