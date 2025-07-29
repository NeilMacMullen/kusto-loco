using System;
using System.Text.Json.Nodes;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

public static class NullableSetBuilderLocator
{

    public static INullableSetBuilder GetFixedNullableSetBuilderForType(Type t, int initialLength)
        => GetNullableSetBuilderForType(t, initialLength, false);

    public static INullableSetBuilder GetExpandableNullableSetBuilderForType(Type t, int initialLength)
        => GetNullableSetBuilderForType(t, initialLength, true);
    
    private static INullableSetBuilder GetNullableSetBuilderForType(Type t, int initialLength,bool canResize)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return new NullableSetBuilderOfbool(initialLength,canResize);
        if (u == typeof(int))
            return new NullableSetBuilderOfint(initialLength, canResize);
        if (u == typeof(long))
            return new NullableSetBuilderOflong(initialLength, canResize);
        if (u == typeof(decimal))
            return new NullableSetBuilderOfdecimal(initialLength, canResize);
        if (u == typeof(double))
            return new NullableSetBuilderOfdouble(initialLength, canResize);
        if (u == typeof(Guid))
            return new NullableSetBuilderOfGuid(initialLength, canResize);
        if (u == typeof(DateTime))
            return new NullableSetBuilderOfDateTime(initialLength, canResize);
        if (u == typeof(TimeSpan))
            return new NullableSetBuilderOfTimeSpan(initialLength, canResize);
        if (u == typeof(string))
            return new NullableSetBuilderOfstring(initialLength, canResize);
        if (u == typeof(JsonNode))
            return new NullableSetBuilderOfJsonNode(initialLength, canResize);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
}
