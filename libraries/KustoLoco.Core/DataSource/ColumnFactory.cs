using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;
using System;
using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.DataSource;

public static class ColumnFactory
{

    public static BaseColumn CreateFromDataSet(INullableSet set)
    {
        return CreateFromDataSet(set.UnderlyingType, set);
    }

    private static BaseColumn CreateFromDataSet(Type t, INullableSet set)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return CreateFromDataSet<bool?>(set);
        if (u == typeof(int))
            return CreateFromDataSet<int?>(set);
        if (u == typeof(long))
            return CreateFromDataSet<long?>(set);
        if (u == typeof(decimal))
            return CreateFromDataSet<decimal?>(set);
        if (u == typeof(double))
            return CreateFromDataSet<double?>(set);
        if (u == typeof(Guid))
            return CreateFromDataSet<Guid?>(set);
        if (u == typeof(DateTime))
            return CreateFromDataSet<DateTime?>(set);
        if (u == typeof(TimeSpan))
            return CreateFromDataSet<TimeSpan?>(set);
        if (u == typeof(string))
            return CreateFromDataSet<string?>(set);
        if (u == typeof(JsonNode))
            return CreateFromDataSet<JsonNode?>(set);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }

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
