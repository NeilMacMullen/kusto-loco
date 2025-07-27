using System;
using System.Text.Json.Nodes;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

public static class NullableSetLocator
{
    public static INullableSet GetNullableForType(Type t, object?[] data)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return new NullableSetOfbool(data);
        if (u == typeof(int))
            return new NullableSetOfint(data);
        if (u == typeof(long))
            return new NullableSetOflong(data);
        if (u == typeof(decimal))
            return new NullableSetOfdecimal(data);
        if (u == typeof(double))
            return new NullableSetOfdouble(data);
        if (u == typeof(Guid))
            return new NullableSetOfGuid(data);
        if (u == typeof(DateTime))
            return new NullableSetOfDateTime(data);
        if (u == typeof(TimeSpan))
            return new NullableSetOfTimeSpan(data);
        if (u == typeof(string))
            return new NullableSetOfstring(data);
        if (u == typeof(JsonNode))
            return new NullableSetOfJsonNode(data);
        

        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
}
