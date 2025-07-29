using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;
using System;
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
            return GenericColumnFactoryOfbool.CreateFromDataSet((NullableSetOfbool)set);
        if (u == typeof(int))
            return GenericColumnFactoryOfint.CreateFromDataSet((NullableSetOfint)set);
        if (u == typeof(long))
            return GenericColumnFactoryOflong.CreateFromDataSet((NullableSetOflong)set);
        if (u == typeof(decimal))
            return GenericColumnFactoryOfdecimal.CreateFromDataSet((NullableSetOfdecimal)set);
        if (u == typeof(double))
            return GenericColumnFactoryOfdouble.CreateFromDataSet((NullableSetOfdouble)set);
        if (u == typeof(Guid))
            return GenericColumnFactoryOfGuid.CreateFromDataSet((NullableSetOfGuid)set);
        if (u == typeof(DateTime))
            return GenericColumnFactoryOfDateTime.CreateFromDataSet((NullableSetOfDateTime)set);
        if (u == typeof(TimeSpan))
            return GenericColumnFactoryOfTimeSpan.CreateFromDataSet((NullableSetOfTimeSpan)set);
        if (u == typeof(string))
            return GenericColumnFactoryOfstring.CreateFromDataSet((NullableSetOfstring)set);
        if (u == typeof(JsonNode))
            return GenericColumnFactoryOfJsonNode.CreateFromDataSet((NullableSetOfJsonNode)set);
        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
  

    public static BaseColumn Inflate(BaseColumn column, int logicalRowCount)
    {
        var value = column.GetRawDataValue(0);
        return ColumnHelpers.CreateFromScalar(value, column.Type, logicalRowCount);
    }
}
