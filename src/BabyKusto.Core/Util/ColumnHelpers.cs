// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Nodes;
using BabyKusto.Core.Extensions;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Util;

public static class ColumnHelpers
{
    public static BaseColumn CreateFromObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int)
        {
            return CreateFromIntsObjectArray(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return CreateFromLongsObjectArray(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return CreateFromDoublesObjectArray(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.Bool)
        {
            return CreateFromBoolsObjectArray(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return CreateFromObjectArray<string>(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return CreateFromObjectArray<DateTime?>(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return CreateFromObjectArray<TimeSpan?>(data, typeSymbol);
        }

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return CreateFromObjectArray<JsonNode?>(data, typeSymbol);
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
            return CreateFromInt(value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return CreateFromLong(value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return CreateFromDouble(value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.Bool)
        {
            return CreateFromBool(value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return CreateFromScalar((string?)value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return CreateFromScalar((DateTime?)value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return CreateFromScalar((TimeSpan?)value, typeSymbol, numRows);
        }

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return CreateFromScalar((JsonNode?)value, typeSymbol, numRows);
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

    public static BaseColumnBuilder CreateBuilder(Type type)
        => CreateBuilder(TypeMapping.SymbolForType(type));


    public static BaseColumnBuilder CreateBuilder(TypeSymbol typeSymbol)
    {
        typeSymbol = typeSymbol.Simplify();
        if (typeSymbol == ScalarTypes.Int)
        {
            return new ColumnBuilder<int?>();
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return new ColumnBuilder<long?>();
        }

        if (typeSymbol == ScalarTypes.Real)
        {
            return new ColumnBuilder<double?>();
        }

        if (typeSymbol == ScalarTypes.Bool)
        {
            return new ColumnBuilder<bool?>();
        }

        if (typeSymbol == ScalarTypes.String)
        {
            return new ColumnBuilder<string?>();
        }

        if (typeSymbol == ScalarTypes.DateTime)
        {
            return new ColumnBuilder<DateTime?>();
        }

        if (typeSymbol == ScalarTypes.TimeSpan)
        {
            return new ColumnBuilder<TimeSpan?>();
        }

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return new ColumnBuilder<JsonNode?>();
        }

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }

    public static BaseColumn MapColumn(BaseColumn other, int offset, int length)
    {
        //TODO - packing the offset and length into a mapping array is just a bit of 
        //a hack to make the internal API easier
        return MapColumn(new[] { other }, new[] { offset, length }.ToImmutableArray(),
            MappingType.Chunk);
    }

    public static BaseColumn MapColumn(BaseColumn other, ImmutableArray<int> mapping)
    {
        return MapColumn(new[] { other }, mapping, MappingType.Arbitrary);
    }

    public static BaseColumn ReassembleInOrder(BaseColumn[] others) =>
        MapColumn(others, ImmutableArray<int>.Empty, MappingType.Reassembly);

    private static TypedBaseColumn<T> Create<T>(ImmutableArray<int> mapping, TypedBaseColumn<T>[] other,
        MappingType mapType)
    {
        return mapType switch
        {
            MappingType.Arbitrary => MapColumn(other[0], mapping),
            MappingType.Chunk => ChunkColumn<T>.Create(mapping[0], mapping[1], other[0]),
            MappingType.Reassembly => new ReassembledChunkColumn<T>(other),
            _ => throw new NotImplementedException()
        };
    }

    private static BaseColumn MapColumn(IEnumerable<BaseColumn> others, ImmutableArray<int> mapping,
        MappingType mapType)
    {
        var typeSymbol = others.First().Type;
        if (typeSymbol == ScalarTypes.Int)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<int?>>().ToArray(), mapType);
        }

        if (typeSymbol == ScalarTypes.Long)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<long?>>().ToArray(), mapType);
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

        if (typeSymbol == ScalarTypes.Dynamic)
        {
            return Create(mapping, others.Cast<TypedBaseColumn<JsonNode?>>().ToArray(), mapType);
        }

        // TODO: Support all data types
        throw new NotImplementedException(
            $"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
    }


    public static TypedBaseColumn<T> MapColumn<T>(TypedBaseColumn<T> other, ImmutableArray<int> mapping) =>
        MappedColumn<T>.Create(mapping, other);

    private static TypedBaseColumn<T> CreateFromObjectArray<T>(object?[] data, TypeSymbol typeSymbol)
    {
        var columnData = new T?[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            columnData[i] = (T?)data[i];
        }

        return new InMemoryColumn<T>(typeSymbol, columnData);
    }

    private static TypedBaseColumn<int?> CreateFromIntsObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        var columnData = new int?[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var item = data[i];
            columnData[i] = item == null ? null : Convert.ToInt32(item);
        }

        return ColumnFactory.Create(typeSymbol, columnData);
    }

    private static TypedBaseColumn<long?> CreateFromLongsObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        var columnData = new long?[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var item = data[i];
            columnData[i] = item == null ? null : Convert.ToInt64(item);
        }

        return ColumnFactory.Create(typeSymbol, columnData);
    }

    private static TypedBaseColumn<double?> CreateFromDoublesObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        var columnData = new double?[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var item = data[i];
            columnData[i] = item == null ? null : Convert.ToDouble(item);
        }

        return ColumnFactory.Create(typeSymbol, columnData);
    }

    private static TypedBaseColumn<bool?> CreateFromBoolsObjectArray(object?[] data, TypeSymbol typeSymbol)
    {
        var columnData = new bool?[data.Length];
        for (var i = 0; i < data.Length; i++)
        {
            var item = data[i];
            columnData[i] = item == null ? null : Convert.ToBoolean(item);
        }

        return ColumnFactory.Create(typeSymbol, columnData);
    }

    private static TypedBaseColumn<T> CreateFromScalar<T>(T value, TypeSymbol typeSymbol, int rowCount) =>
        new SingleValueColumn<T>(typeSymbol, value, rowCount);

    private static TypedBaseColumn<int?> CreateFromInt(object? value, TypeSymbol typeSymbol, int rowCount) =>
        CreateFromScalar<int?>(value == null ? null : Convert.ToInt32(value), typeSymbol, rowCount);

    private static TypedBaseColumn<long?> CreateFromLong(object? value, TypeSymbol typeSymbol, int rowCount) =>
        CreateFromScalar<long?>(value == null ? null : Convert.ToInt64(value), typeSymbol, rowCount);

    private static TypedBaseColumn<double?> CreateFromDouble(object? value, TypeSymbol typeSymbol, int rowCount) =>
        CreateFromScalar<double?>(value == null ? null : Convert.ToDouble(value), typeSymbol, rowCount);

    private static TypedBaseColumn<bool?> CreateFromBool(object? value, TypeSymbol typeSymbol, int rowCount) =>
        CreateFromScalar<bool?>(value == null ? null : Convert.ToBoolean(value), typeSymbol, rowCount);

    private enum MappingType
    {
        Arbitrary,
        Chunk,
        Reassembly
    }
}