// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Extensions;

namespace KustoLoco.Core.Util;

public static class ColumnHelpers
{
    public static BaseColumn CreateFromObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int) return CreateFromIntsObjectArray(data);

        if (typeSymbol == ScalarTypes.Long) return CreateFromLongsObjectArray(data);

        if (typeSymbol == ScalarTypes.Real) return CreateFromDoublesObjectArray(data);

        if (typeSymbol == ScalarTypes.Decimal) return CreateFromDecimalsObjectArray(data);

        if (typeSymbol == ScalarTypes.Bool) return CreateFromBoolsObjectArray(data);

        if (typeSymbol == ScalarTypes.String) return new GenericInMemoryColumnOfstring(data);

        if (typeSymbol == ScalarTypes.DateTime) return new GenericInMemoryColumnOfDateTime(data);

        if (typeSymbol == ScalarTypes.TimeSpan) return new GenericInMemoryColumnOfTimeSpan(data);

        if (typeSymbol == ScalarTypes.Guid) return new GenericInMemoryColumnOfGuid(data);

        if (typeSymbol == ScalarTypes.Dynamic) return CreateFroDynamicObjectArray(data);

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
    public static BaseColumnBuilder[] CreateBuildersForTable(TableSymbol table) =>
        table.Columns.Select(c => CreateBuilder(c.Type)).ToArray();

    public static BaseColumnBuilder CreateBuilder(Type type, string name) =>
        CreateBuilder(TypeMapping.SymbolForType(type), name);

    public static BaseColumnBuilder CreateBuilder(TypeSymbol typeSymbol)
        => CreateBuilder(typeSymbol, string.Empty);

    public static BaseColumnBuilder CreateBuilder(TypeSymbol typeSymbol, string name)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int) return new GenericColumnBuilderOfint(name);

        if (typeSymbol == ScalarTypes.Long) return new GenericColumnBuilderOflong(name);

        if (typeSymbol == ScalarTypes.Decimal) return new GenericColumnBuilderOfdecimal(name);

        if (typeSymbol == ScalarTypes.Real) return new GenericColumnBuilderOfdouble(name);

        if (typeSymbol == ScalarTypes.Bool) return new GenericColumnBuilderOfbool(name);

        if (typeSymbol == ScalarTypes.String) return new GenericColumnBuilderOfstring(name);

        if (typeSymbol == ScalarTypes.DateTime) return new GenericColumnBuilderOfDateTime(name);

        if (typeSymbol == ScalarTypes.Guid) return new GenericColumnBuilderOfGuid(name);

        if (typeSymbol == ScalarTypes.TimeSpan) return new GenericColumnBuilderOfTimeSpan(name);

        if (typeSymbol == ScalarTypes.Dynamic) return new GenericColumnBuilderOfJsonNode(name);

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    public static BaseColumn MapColumn(BaseColumn other, int offset, int length) =>
        //TODO - packing the offset and length into a mapping array is just a bit of 
        //a hack to make the internal API easier
        MapColumn([other], [offset, length],
            MappingType.Chunk);

   
    public static BaseColumn ReassembleInOrder(BaseColumn[] others)
        => MapColumn(others, ImmutableArray<int>.Empty, MappingType.Reassembly);

    private static BaseColumn Create<T>(ImmutableArray<int> mapping, BaseColumn[] other,
        MappingType mapType) =>
        mapType switch
        {
            MappingType.Arbitrary => MapColumn(other[0], mapping),
            MappingType.Chunk => ChunkCreate(mapping[0], mapping[1], other[0]),
            MappingType.Reassembly => Reassemble(other),
            _ => throw new NotImplementedException()
        };

    public static BaseColumn ChunkCreate(int a,int b,BaseColumn col)
    {
        var typeSymbol = col.Type;
        if (typeSymbol == ScalarTypes.Int)
            return GenericChunkColumnOfint.Create(a,b,col);

        if (typeSymbol == ScalarTypes.Long)
            return GenericChunkColumnOflong.Create(a, b, col);

        if (typeSymbol == ScalarTypes.Decimal)
            return GenericChunkColumnOfdecimal.Create(a, b, col);
        if (typeSymbol == ScalarTypes.Real)
            return GenericChunkColumnOfdouble.Create(a, b, col);

        if (typeSymbol == ScalarTypes.Bool)
            return GenericChunkColumnOfbool.Create(a, b, col); ;

        if (typeSymbol == ScalarTypes.String)
            return GenericChunkColumnOfstring.Create(a, b, col);
        if (typeSymbol == ScalarTypes.DateTime)
            return GenericChunkColumnOfDateTime.Create(a, b, col);

        if (typeSymbol == ScalarTypes.TimeSpan)
            return GenericChunkColumnOfTimeSpan.Create(a, b, col);

        if (typeSymbol == ScalarTypes.Guid)
            return GenericChunkColumnOfGuid.Create(a, b, col);
        if (typeSymbol == ScalarTypes.Dynamic)
            return GenericChunkColumnOfJsonNode.Create(a, b, col);

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }


    public static BaseColumn MapColumn(BaseColumn other, ImmutableArray<int> mapping)
    {
            var typeSymbol = other.Type;
            if (typeSymbol == ScalarTypes.Int) return GenericMappedColumnOfint.Create(mapping, other);

            if (typeSymbol == ScalarTypes.Long)
            return GenericMappedColumnOflong.Create(mapping, other);

        if (typeSymbol == ScalarTypes.Decimal)
            return GenericMappedColumnOfdecimal.Create(mapping, other);

        if (typeSymbol == ScalarTypes.Real)
            return GenericMappedColumnOfdouble.Create(mapping, other);

        if (typeSymbol == ScalarTypes.Bool)
            return GenericMappedColumnOfbool.Create(mapping, other);

        if (typeSymbol == ScalarTypes.String)
            return GenericMappedColumnOfstring.Create(mapping, other);

        if (typeSymbol == ScalarTypes.DateTime)
            return GenericMappedColumnOfDateTime.Create(mapping, other);

        if (typeSymbol == ScalarTypes.TimeSpan)
            return GenericMappedColumnOfTimeSpan.Create(mapping, other);

        if (typeSymbol == ScalarTypes.Guid)
            return GenericMappedColumnOfGuid.Create(mapping, other);
        if (typeSymbol == ScalarTypes.Dynamic)
            return GenericMappedColumnOfJsonNode.Create(mapping, other);

        // TODO: Support all data types
        throw new NotImplementedException(
                $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    public static BaseColumn Reassemble(BaseColumn[] other)
    {
        var typeSymbol = other[0].Type;
        if (typeSymbol == ScalarTypes.Int)
            return GenericReassembledChunkColumnOfint.Create(other);

        if (typeSymbol == ScalarTypes.Long)
            return GenericReassembledChunkColumnOflong.Create(other);

        if (typeSymbol == ScalarTypes.Decimal)
            return GenericReassembledChunkColumnOfdecimal.Create(other);

        if (typeSymbol == ScalarTypes.Real)
            return GenericReassembledChunkColumnOfdouble.Create(other);

        if (typeSymbol == ScalarTypes.Bool)
            return GenericReassembledChunkColumnOfbool.Create(other);

        if (typeSymbol == ScalarTypes.String)
            return GenericReassembledChunkColumnOfstring.Create(other);

        if (typeSymbol == ScalarTypes.DateTime)
            return GenericReassembledChunkColumnOfDateTime.Create(other);

        if (typeSymbol == ScalarTypes.TimeSpan)
            return GenericReassembledChunkColumnOfTimeSpan.Create(other);

        if (typeSymbol == ScalarTypes.Guid)
            return GenericReassembledChunkColumnOfGuid.Create(other);
        if (typeSymbol == ScalarTypes.Dynamic)
            return GenericReassembledChunkColumnOfJsonNode.Create(other);

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    public static BaseColumn CreateFromScalar(object? value, TypeSymbol type, int logicalRowCount)
    {
        var columnType = TypeMapping.UnderlyingTypeForSymbol(type);
        return SingleValueColumnLocator.CreateSingleValueColumn(columnType, value, logicalRowCount);
    }

    public static BaseColumn CreateFromScalar(BaseColumn column, int logicalRowCount)
    {
        var value = column.GetRawDataValue(0);
        return CreateFromScalar(value, column.Type, logicalRowCount);
    }

    private static BaseColumn MapColumn(IEnumerable<BaseColumn> others, ImmutableArray<int> mapping,
        MappingType mapType)
    {
        var typeSymbol = others.First().Type;
        if (typeSymbol == ScalarTypes.Int)
            return Create<int>(mapping, others.Cast<GenericTypedBaseColumnOfint>().ToArray(), mapType); 

        if (typeSymbol == ScalarTypes.Long)
            return Create<long>(mapping, others.Cast<GenericTypedBaseColumnOflong>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.Decimal)
            return Create<decimal>(mapping, others.Cast<GenericTypedBaseColumnOfdecimal>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.Real)
            return Create<double>(mapping, others.Cast<GenericTypedBaseColumnOfdouble>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.Bool)
            return Create<bool>(mapping, others.Cast<GenericTypedBaseColumnOfbool>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.String)
            return Create<string>(mapping, others.Cast<GenericTypedBaseColumnOfstring>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.DateTime)
            return Create<DateTime>(mapping, others.Cast<GenericTypedBaseColumnOfDateTime>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.TimeSpan)
            return Create<TimeSpan>(mapping, others.Cast<GenericTypedBaseColumnOfTimeSpan>().ToArray(), mapType);

        if (typeSymbol == ScalarTypes.Guid)
            return Create<Guid>(mapping, others.Cast<GenericTypedBaseColumnOfGuid>().ToArray(), mapType);
        if (typeSymbol == ScalarTypes.Dynamic)
            return Create<JsonNode>(mapping, others.Cast<GenericTypedBaseColumnOfJsonNode>().ToArray(), mapType);

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    /*
    public static GenericTypedBaseColumn<T> MapColumn<T>(TypedBaseColumn<T> other, ImmutableArray<int> mapping)
        => MappedColumn<T>.Create(mapping, other);
    */

    private static GenericTypedBaseColumnOfint
        CreateFromIntsObjectArray(object?[] data)
    {
        var columnData = NullableSetBuilderOfint.CreateFixed(data.Length);
        foreach (var item in data)
        {
            int? d = item == null
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

    private static GenericTypedBaseColumnOfJsonNode CreateFroDynamicObjectArray(object?[] data) =>
        GenericColumnFactoryOfJsonNode.CreateFromObjects(data);

    /*
    private static GenericTypedBaseColumn<T> CreateSingleValueColumn<T>(T value, int rowCount)
        => new GenericSingleValueColumn<T>(value, rowCount);
    */

    private enum MappingType
    {
        Arbitrary,
        Chunk,
        Reassembly
    }
}
