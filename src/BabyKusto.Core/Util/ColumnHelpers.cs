// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using BabyKusto.Core.Extensions;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Util
{
    public static class ColumnHelpers
    {
        public static Column CreateFromObjectArray(object?[] data, TypeSymbol typeSymbol)
        {
            typeSymbol = typeSymbol.Simplify();
            if (typeSymbol == ScalarTypes.Int)
            {
                return CreateFromIntsObjectArray(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Long)
            {
                return CreateFromLongsObjectArray(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Real)
            {
                return CreateFromDoublesObjectArray(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Bool)
            {
                return CreateFromBoolsObjectArray(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.String)
            {
                return CreateFromObjectArray<string>(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.DateTime)
            {
                return CreateFromObjectArray<DateTime?>(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.TimeSpan)
            {
                return CreateFromObjectArray<TimeSpan?>(data, typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Dynamic)
            {
                return CreateFromObjectArray<JsonNode?>(data, typeSymbol);
            }
            else
            {
                // TODO: Support all data types
                throw new NotImplementedException($"Unsupported scalar type to create column from: {SchemaDisplay.GetText(typeSymbol)}");
            }
        }

        public static Column CreateFromScalar(object? value, TypeSymbol typeSymbol, int numRows)
        {
            typeSymbol = typeSymbol.Simplify();
            if (typeSymbol == ScalarTypes.Int)
            {
                return CreateFromInt(value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.Long)
            {
                return CreateFromLong(value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.Real)
            {
                return CreateFromDouble(value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.Bool)
            {
                return CreateFromBool(value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.String)
            {
                return CreateFromScalar<string?>((string?)value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.DateTime)
            {
                return CreateFromScalar<DateTime?>((DateTime?)value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.TimeSpan)
            {
                return CreateFromScalar<TimeSpan?>((TimeSpan?)value, typeSymbol, numRows);
            }
            else if (typeSymbol == ScalarTypes.Dynamic)
            {
                return CreateFromScalar<JsonNode?>((JsonNode?)value, typeSymbol, numRows);
            }
            else
            {
                // TODO: Support all data types
                throw new NotImplementedException($"Unsupported scalar type to create column from: {SchemaDisplay.GetText(typeSymbol)}");
            }
        }

        public static ColumnBuilder CreateBuilder(TypeSymbol typeSymbol)
        {
            typeSymbol = typeSymbol.Simplify();
            if (typeSymbol == ScalarTypes.Int)
            {
                return new ColumnBuilder<int?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Long)
            {
                return new ColumnBuilder<long?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Real)
            {
                return new ColumnBuilder<double?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Bool)
            {
                return new ColumnBuilder<bool?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.String)
            {
                return new ColumnBuilder<string?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.DateTime)
            {
                return new ColumnBuilder<DateTime?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.TimeSpan)
            {
                return new ColumnBuilder<TimeSpan?>(typeSymbol);
            }
            else if (typeSymbol == ScalarTypes.Dynamic)
            {
                return new ColumnBuilder<JsonNode?>(typeSymbol);
            }
            else
            {
                // TODO: Support all data types
                throw new NotImplementedException($"Unsupported scalar type to create column builder from: {SchemaDisplay.GetText(typeSymbol)}");
            }
        }

        private static Column<T> CreateFromObjectArray<T>(object?[] data, TypeSymbol typeSymbol)
        {
            var columnData = new T?[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                columnData[i] = (T?)data[i];
            }

            return new Column<T>(typeSymbol, columnData);
        }

        private static Column<int?> CreateFromIntsObjectArray(object?[] data, TypeSymbol typeSymbol)
        {
            var columnData = new int?[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var item = data[i];
                columnData[i] = item == null ? null : Convert.ToInt32(item);
            }

            return Column.Create(typeSymbol, columnData);
        }

        private static Column<long?> CreateFromLongsObjectArray(object?[] data, TypeSymbol typeSymbol)
        {
            var columnData = new long?[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var item = data[i];
                columnData[i] = item == null ? null : Convert.ToInt64(item);
            }

            return Column.Create(typeSymbol, columnData);
        }

        private static Column<double?> CreateFromDoublesObjectArray(object?[] data, TypeSymbol typeSymbol)
        {
            var columnData = new double?[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var item = data[i];
                columnData[i] = item == null ? null : Convert.ToDouble(item);
            }

            return Column.Create(typeSymbol, columnData);
        }

        private static Column<bool?> CreateFromBoolsObjectArray(object?[] data, TypeSymbol typeSymbol)
        {
            var columnData = new bool?[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                var item = data[i];
                columnData[i] = item == null ? null : Convert.ToBoolean(item);
            }

            return Column.Create(typeSymbol, columnData);
        }

        private static Column<T> CreateFromScalar<T>(T value, TypeSymbol typeSymbol, int rowCount)
        {
            var columnData = new T[rowCount];
            for (var i = 0; i < rowCount; i++)
            {
                columnData[i] = value;
            }

            return new Column<T>(typeSymbol, columnData);
        }

        private static Column<int?> CreateFromInt(object? value, TypeSymbol typeSymbol, int rowCount)
        {
            return CreateFromScalar<int?>(value == null ? null : Convert.ToInt32(value), typeSymbol, rowCount);
        }

        private static Column<long?> CreateFromLong(object? value, TypeSymbol typeSymbol, int rowCount)
        {
            return CreateFromScalar<long?>(value == null ? null : Convert.ToInt64(value), typeSymbol, rowCount);
        }

        private static Column<double?> CreateFromDouble(object? value, TypeSymbol typeSymbol, int rowCount)
        {
            return CreateFromScalar<double?>(value == null ? null : Convert.ToDouble(value), typeSymbol, rowCount);
        }

        private static Column<bool?> CreateFromBool(object? value, TypeSymbol typeSymbol, int rowCount)
        {
            return CreateFromScalar<bool?>(value == null ? null : Convert.ToBoolean(value), typeSymbol, rowCount);
        }
    }
}
