using System;
using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public abstract class BaseColumn
{
    protected BaseColumn(TypeSymbol type) => Type = type;
    public TypeSymbol Type { get; }
    public abstract int RowCount { get; }

    public abstract object? GetRawDataValue(int index);

    public abstract BaseColumn Slice(int start, int end);
    public abstract void ForEach(Action<object?> action);
}