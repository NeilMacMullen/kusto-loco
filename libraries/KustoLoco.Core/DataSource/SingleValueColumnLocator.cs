using System;
using System.Text.Json.Nodes;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource;

public class SingleValueColumnLocator
{
    public static BaseColumn CreateSingleValueColumn(Type t, object? data,int rowCount)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return new GenericSingleValueColumnOfbool(data,rowCount);
        if (u == typeof(int))
            return new GenericSingleValueColumnOfint(data, rowCount);
        if (u == typeof(long))
            return new GenericSingleValueColumnOflong(data, rowCount);
        if (u == typeof(decimal))
            return new GenericSingleValueColumnOfdecimal(data, rowCount);
        if (u == typeof(double))
            return new GenericSingleValueColumnOfdouble(data, rowCount);
        if (u == typeof(Guid))
            return new GenericSingleValueColumnOfGuid(data, rowCount);
        if (u == typeof(DateTime))
            return new GenericSingleValueColumnOfDateTime(data, rowCount);
        if (u == typeof(TimeSpan))
            return new GenericSingleValueColumnOfTimeSpan(data, rowCount);
        if (u == typeof(string))
            return new GenericSingleValueColumnOfstring(data, rowCount);
        if (u == typeof(JsonNode))
            return new GenericSingleValueColumnOfJsonNode(data, rowCount);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
}
