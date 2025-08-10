using System;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Util;

public abstract class BaseColumnBuilder
{
    protected INullableSet? _nullableSet;
    public abstract int RowCount { get; }
    public string Name { get; protected set; } = string.Empty;
    public abstract void Add(object? value);
    public abstract BaseColumn ToColumn();
   
    public void PadTo(int size)
    {
        //pad with nulls 
        while (RowCount < size) Add(null);
    }

    public void AddAt(object? value, int rowIndex)
    {
        PadTo(rowIndex);
        Add(value);
    }
    public void AddNullableSet(INullableSet set)
    {
        _nullableSet = set;
    }
}
