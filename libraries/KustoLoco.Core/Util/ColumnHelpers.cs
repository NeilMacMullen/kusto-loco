// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using KustoLoco.Core.Extensions;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Util;

public static class ColumnHelpers
{
    public static BaseColumn CreateFromObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int)
        {
            return CreateFromIntsObjectArray(data);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return CreateFromLongsObjectArray(data);
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return CreateFromDoublesObjectArray(data);
        }

        if (typeSymbol == ScalarTypes.Decimal)
        {
            return CreateFromDecimalsObjectArray(data);
        }

        if (typeSymbol == ScalarTypes.Bool)
        {
            return CreateFromBoolsObjectArray(data);
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return CreateFromObjectArray<string>(data);
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return CreateFromObjectArray<DateTime?>(data);
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return CreateFromObjectArray<TimeSpan?>(data);
        }

        if (typeSymbol == ScalarTypes.Guid)
        {
            return CreateFromObjectArray<Guid?>(data);
        }

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return CreateFroDynamicObjectArray(data);
        }

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    public static BaseColumn CreateFromScalar(object? value, TypeSymbol typeSymbol, int numRows)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int)
        {
            return CreateFromInt(value, numRows);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return CreateFromLong(value, numRows);
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return CreateFromDouble(value, numRows);
        }

        if (typeSymbol == ScalarTypes.Decimal)
        {
            return CreateFromDecimal(value, numRows);
        }


        if (typeSymbol == ScalarTypes.Bool)
        {
            return CreateFromBool(value, numRows);
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return CreateFromScalar((string?)value, numRows);
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return CreateFromScalar((DateTime?)value, numRows);
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return CreateFromScalar((TimeSpan?)value, numRows);
        }

        if (typeSymbol == ScalarTypes.Guid)
        {
            return CreateFromScalar((Guid?)value, numRows);
        }

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return CreateFromScalar((JsonNode?)value, numRows);
        }

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    /// <summary>
    ///     Creates column builds for a Table symbol
    /// </summary>
    /// <remarks>
    ///     This is useful when we want to populate a set of columns based on an expected tabular
    ///     return type
    /// </remarks>
    public static BaseColumnBuilder[] CreateBuildersForTable(TableSymbol table)
    {
        return table.Columns.Select(c => CreateBuilder(c.Type)).ToArray();
    }

    public static BaseColumnBuilder CreateBuilder(Type type,string name) => CreateBuilder(TypeMapping.SymbolForType(type),name);

    public static BaseColumnBuilder CreateBuilder(TypeSymbol typeSymbol)
    => CreateBuilder(typeSymbol, string.Empty);

    public static BaseColumnBuilder CreateBuilder(TypeSymbol typeSymbol,string name)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int)
        {
            return new GenericColumnBuilderOfint(name);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return new GenericColumnBuilderOflong(name);
        }

        if (typeSymbol == ScalarTypes.Decimal)
        {
            return new GenericColumnBuilderOfdecimal(name);
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return new GenericColumnBuilderOfdouble(name);
        }

        if (typeSymbol == ScalarTypes.Bool)
        {
            return new GenericColumnBuilderOfbool(name);
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return new GenericColumnBuilderOfstring(name);
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return new GenericColumnBuilderOfDateTime(name);
        }

        if (typeSymbol == ScalarTypes.Guid)
        {
            return new GenericColumnBuilderOfGuid(name);
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return new GenericColumnBuilderOfTimeSpan(name);
        }

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return new GenericColumnBuilderOfJsonNode(name);
        }

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    public static BaseColumn MapColumn(BaseColumn other, int offset, int length)
    {
        //TODO - packing the offset and length into a mapping array is just a bit of 
        //a hack to make the internal API easier
        return MapColumn([other], [offset, length],
            MappingType.Chunk);
    }

    public static BaseColumn MapColumn(BaseColumn other, ImmutableArray<int> mapping)
    {
        return MapColumn([other], mapping, MappingType.Arbitrary);
    }

    public static BaseColumn ReassembleInOrder(BaseColumn[] others)
        => MapColumn(others, ImmutableArray<int>.Empty, MappingType.Reassembly);

    private static BaseColumn Create<T>(ImmutableArray<int> mapping, BaseColumn[] other,
        MappingType mapType)
    {
        return mapType switch
               {
                   MappingType.Arbitrary => MapColumn(other[0], mapping),
                   MappingType.Chunk => ChunkColumn<T>.Create(mapping[0], mapping[1], other[0]),
                   MappingType.Reassembly =>  ReassembledChunkColumn<T>.Create(other), 
                   _ => throw new NotImplementedException()
               };
    }

    private static BaseColumn MapColumn(IEnumerable<BaseColumn> others, ImmutableArray<int> mapping,
        MappingType mapType)
    {
        var typeSymbol = others.First().Type;
        return Create(mapping, others.ToArray(), mapType);
        if (typeSymbol == ScalarTypes.Int)
        {
            return Create(mapping, others.ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<long?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.Decimal)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<decimal?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<double?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.Bool)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<bool?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<string?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<DateTime?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<TimeSpan?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.Guid)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<Guid?>>().ToArray(), mapType);
        }
        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<JsonNode?>>().ToArray(), mapType);
        }

        // TODO: Support all data types
        throw new NotImplementedException(
                                          $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }


    public static TypedBaseColumn<T> MapColumn<T>(TypedBaseColumn<T> other, ImmutableArray<int> mapping)
        => MappedColumn<T>.Create(mapping, other);

    private static TypedBaseColumn<T> CreateFromObjectArray<T>(object?[] data)
    {

        return new InMemoryColumn<T>(data);
    }

    private static GenericTypedBaseColumnOfint
        CreateFromIntsObjectArray(object?[] data)
    {
        var columnData = NullableSetBuilderOfint.CreateFixed(data.Length);
        foreach (var item in data)
        {
            int? d  = item == null
                ? null
                : Convert.ToInt32(item);
            columnData.Add(d);
        }
        return GenericColumnFactoryOfint.CreateFromDataSet(columnData.ToNullableSet());
    }

    private static GenericTypedBaseColumnOflong CreateFromLongsObjectArray(object?[] data)
    {
        var columnData = NullableSetBuilderOflong.CreateFixed(data.Length);
        foreach (var item in data)
        {
            long? d = item == null
                ? null
                : Convert.ToInt64(item);
            columnData.Add(d);
        }
        return GenericColumnFactoryOflong.CreateFromDataSet(columnData.ToNullableSet());
    }

    private static GenericTypedBaseColumnOfdouble CreateFromDoublesObjectArray(object?[] data)
    {
        var columnData = NullableSetBuilderOfdouble.CreateFixed(data.Length);
        foreach (var item in data)
        {
            double? d = item == null
                ? null
                : Convert.ToDouble(item);
            columnData.Add(d);
        }
        return GenericColumnFactoryOfdouble.CreateFromDataSet(columnData.ToNullableSet());
    }

    private static GenericTypedBaseColumnOfdecimal CreateFromDecimalsObjectArray(object?[] data)
    {
        var columnData = NullableSetBuilderOfdecimal.CreateFixed(data.Length);
        foreach (var item in data)
        {
            decimal? d = item == null
                ? null
                : Convert.ToDecimal(item);
            columnData.Add(d);
        }
        return GenericColumnFactoryOfdecimal.CreateFromDataSet(columnData.ToNullableSet());
    }

    private static GenericTypedBaseColumnOfbool CreateFromBoolsObjectArray(object?[] data)
    {
        var columnData = NullableSetBuilderOfbool.CreateFixed(data.Length);
        foreach (var item in data)
        {
            bool? d = item == null
                ? null
                : Convert.ToBoolean(item);
            columnData.Add(d);
        }
        return GenericColumnFactoryOfbool.CreateFromDataSet(columnData.ToNullableSet()); 
    }

    private static GenericTypedBaseColumnOfJsonNode CreateFroDynamicObjectArray(object?[] data)
    {
        return GenericColumnFactoryOfJsonNode.CreateFromObjects(data);
    }

    private static TypedBaseColumn<T> CreateFromScalar<T>(T value, int rowCount)
        => new SingleValueColumn<T>(value, rowCount);

    private static TypedBaseColumn<int?> CreateFromInt(object? value, int rowCount)
        => CreateFromScalar<int?>(value == null
                                      ? null
                                      : Convert.ToInt32(value), rowCount);

    private static TypedBaseColumn<long?> CreateFromLong(object? value, int rowCount)
        => CreateFromScalar<long?>(value == null
                                       ? null
                                       : Convert.ToInt64(value), rowCount);

    private static TypedBaseColumn<double?> CreateFromDouble(object? value, int rowCount)
        => CreateFromScalar<double?>(value == null
                                         ? null
                                         : Convert.ToDouble(value), rowCount);

    private static TypedBaseColumn<decimal?> CreateFromDecimal(object? value, int rowCount)
        => CreateFromScalar<decimal?>(value == null
            ? null
            : Convert.ToDecimal(value), rowCount);


    private static TypedBaseColumn<bool?> CreateFromBool(object? value, int rowCount)
        => CreateFromScalar<bool?>(value == null
                                       ? null
                                       : Convert.ToBoolean(value), rowCount);

    private enum MappingType
    {
        Arbitrary,
        Chunk,
        Reassembly
    }
}
