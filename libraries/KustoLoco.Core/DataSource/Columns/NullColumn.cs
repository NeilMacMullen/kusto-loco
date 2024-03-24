using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.DataSource.Columns;

public class NullColumn : BaseColumn
{
    public static readonly NullColumn Instance = new();

    private NullColumn() : base(ScalarTypes.Null)
    {
    }

    public override int RowCount => 0;


    public override object? GetRawDataValue(int index) => throw new InvalidOperationException();

    public override BaseColumn Slice(int start, int end) => throw new InvalidOperationException();


    public override void ForEach(Action<object?> action)
    {
        throw new InvalidOperationException();
    }
}