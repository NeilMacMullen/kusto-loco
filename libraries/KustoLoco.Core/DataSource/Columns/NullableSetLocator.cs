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
            return NullableSetOfbool.FromObjectsOfCorrectType(data);
        if (u == typeof(int))
            return NullableSetOfint.FromObjectsOfCorrectType(data);
        if (u == typeof(long))
            return NullableSetOflong.FromObjectsOfCorrectType(data);
        if (u == typeof(decimal))
            return NullableSetOfdecimal.FromObjectsOfCorrectType(data);
        if (u == typeof(double))
            return NullableSetOfdouble.FromObjectsOfCorrectType(data);
        if (u == typeof(Guid))
            return  NullableSetOfGuid.FromObjectsOfCorrectType(data);
        if (u == typeof(DateTime))
            return  NullableSetOfDateTime.FromObjectsOfCorrectType(data);
        if (u == typeof(TimeSpan))
            return  NullableSetOfTimeSpan.FromObjectsOfCorrectType(data);
        if (u == typeof(string))
            return  NullableSetOfstring.FromObjectsOfCorrectType(data);
        if (u == typeof(JsonNode))
            return  NullableSetOfJsonNode.FromObjectsOfCorrectType(data);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }

    public static INullableSet GetNullableForTypeAndBaseArray(Type t, Array data)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return NullableSetOfbool.CreateFromBaseArray(data);
        if (u == typeof(int))
            return NullableSetOfint.CreateFromBaseArray(data);
        if (u == typeof(long))
            return NullableSetOflong.CreateFromBaseArray(data);
        if (u == typeof(decimal))
            return NullableSetOfdecimal.CreateFromBaseArray(data);
        if (u == typeof(double))
            return NullableSetOfdouble.CreateFromBaseArray(data);
        if (u == typeof(Guid))
            return NullableSetOfGuid.CreateFromBaseArray(data);
        if (u == typeof(DateTime))
            return NullableSetOfDateTime.CreateFromBaseArray(data);
        if (u == typeof(TimeSpan))
            return NullableSetOfTimeSpan.CreateFromBaseArray(data);
        if (u == typeof(string))
            return NullableSetOfstring.CreateFromBaseArray(data);
        if (u == typeof(JsonNode))
            return NullableSetOfJsonNode.CreateFromBaseArray(data);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
}
