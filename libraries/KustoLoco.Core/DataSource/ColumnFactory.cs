using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;
using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.DataSource;

public static class ColumnFactoryLocator
{
    public static BaseColumn CreateFromDataSet(Type t, INullableSet set)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return ColumnFactory.CreateFromDataSet<bool?>(set);
        if (u == typeof(int))
            return ColumnFactory.CreateFromDataSet<int?>(set);
        if (u == typeof(long))
            return ColumnFactory.CreateFromDataSet<long?>(set);
        if (u == typeof(decimal))
            return ColumnFactory.CreateFromDataSet<decimal?>(set);
        if (u == typeof(double))
            return ColumnFactory.CreateFromDataSet<double?>(set);
        if (u == typeof(Guid))
            return ColumnFactory.CreateFromDataSet<Guid?>(set);
        if (u == typeof(DateTime))
            return ColumnFactory.CreateFromDataSet<DateTime?>(set);
        if (u == typeof(TimeSpan))
            return ColumnFactory.CreateFromDataSet<TimeSpan?>(set);
        if (u == typeof(string))
            return ColumnFactory.CreateFromDataSet<string?>(set);
        if (u == typeof(JsonNode))
            return ColumnFactory.CreateFromDataSet<JsonNode?>(set);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }

}


public static class ColumnFactory
{
    public static TypedBaseColumn<T> CreateFromObjects<T>(object?[] data) =>
        data.Length == 1
            ? new SingleValueColumn<T>(data[0], 1)
            : new InMemoryColumn<T>(data);

    public static TypedBaseColumn<T> CreateFromDataSet<T>(INullableSet data) =>
        data.Length == 1
            ? new SingleValueColumn<T>(data.NullableValue(0), 1)
            : new InMemoryColumn<T>(data);


    public static TypedBaseColumn<T> Create<T>(T?[] data) =>
        CreateFromObjects<T>(data.Cast<object?>().ToArray());

    

    public static BaseColumn Inflate(BaseColumn column, int logicalRowCount)
    {
        var value = column.GetRawDataValue(0);
        return ColumnHelpers.CreateFromScalar(value, column.Type, logicalRowCount);
    }
}
