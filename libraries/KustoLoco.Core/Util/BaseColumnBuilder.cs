using System;
using KustoLoco.Core.DataSource.Columns;
using RTools_NTS.Util;

namespace KustoLoco.Core.Util;

public abstract class BaseColumnBuilder
{
    public abstract int RowCount { get; }
    public abstract object? this[int index] { get; }
    public abstract void Add(object? value);
    public abstract void AddRange(BaseColumnBuilder other);
    public abstract BaseColumn ToColumn();
    public string Name { get; protected set; } = string.Empty;
    public abstract Array GetDataAsArray();

    public void PadTo(int size)
    {
        //pad with nulls 
        while (RowCount < size)
        {
            Add(null);
        }
    }

    public void AddAt(object? value, int rowIndex)
    {
        PadTo(rowIndex);
        Add(value);
    }
}
