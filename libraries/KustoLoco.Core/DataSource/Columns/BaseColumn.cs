using System;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.DataSource.Columns;

[Flags]
public enum ColumnHints
{
    HoldsSingleValue = (1 << 1)
}

public abstract class BaseColumn
{
    protected ColumnHints hints;
    protected BaseColumn(TypeSymbol type) => Type = type;
    public TypeSymbol Type { get; }

    public bool IsSingleValue => hints.HasFlag(ColumnHints.HoldsSingleValue)
                                 | (RowCount == 1);

    public abstract int RowCount { get; }

    public abstract object? GetRawDataValue(int index);

    public abstract BaseColumn Slice(int start, int end);
    public abstract void ForEach(Action<object?> action);
}