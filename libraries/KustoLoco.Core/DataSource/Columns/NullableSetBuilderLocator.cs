using System;
using System.Text.Json.Nodes;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

public static class NullableSetBuilderLocator
{
    public static INullableSetBuilder GetNullableSetBuilderForType(Type t, int initialLength)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return new NullableSetBuilderOfbool(initialLength);
        if (u == typeof(int))
            return new NullableSetBuilderOfint(initialLength);
        if (u == typeof(long))
            return new NullableSetBuilderOflong(initialLength);
        if (u == typeof(decimal))
            return new NullableSetBuilderOfdecimal(initialLength);
        if (u == typeof(double))
            return new NullableSetBuilderOfdouble(initialLength);
        if (u == typeof(Guid))
            return new NullableSetBuilderOfGuid(initialLength);
        if (u == typeof(DateTime))
            return new NullableSetBuilderOfDateTime(initialLength);
        if (u == typeof(TimeSpan))
            return new NullableSetBuilderOfTimeSpan(initialLength);
        if (u == typeof(string))
            return new NullableSetBuilderOfstring(initialLength);
        if (u == typeof(JsonNode))
            return new NullableSetBuilderOfJsonNode(initialLength);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
}
