// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class ToStringFromIntFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dynamicValue = (int?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? Impl(int? input)
            => input.HasValue
                   ? input.Value.ToString()
                   : string.Empty;
    }

    internal class ToStringFromLongFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dynamicValue = (long?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? Impl(long? input)
            => input.HasValue
                   ? input.Value.ToString()
                   : string.Empty;
    }

    internal class ToStringFromRealFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dynamicValue = (double?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? Impl(double? input)
            => input.HasValue
                   ? input.Value.ToString()
                   : string.Empty;
    }

    internal class ToStringFromTimeSpanFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dynamicValue = (TimeSpan?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<TimeSpan?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? Impl(TimeSpan? input)
            => input.HasValue
                   ? input.Value.ToString()
                   : string.Empty;
    }

    internal class ToStringFromDateTimeFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dynamicValue = (DateTime?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<DateTime?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? Impl(DateTime? input)
            => input == null
                   ? string.Empty
                   : input.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
    }

    internal class ToStringFromDynamicFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var dynamicValue = (JsonNode?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, Impl(dynamicValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<JsonNode?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string? Impl(JsonNode? input)
        {
            if (input != null)
            {
                if (input is JsonValue valueNode)
                {
                    var value = valueNode.GetValue<object>();
                    if (value is JsonElement element)
                    {
                        if (element.ValueKind == JsonValueKind.String)
                        {
                            return element.GetString();
                        }

                        if (element.ValueKind == JsonValueKind.Null)
                        {
                            return string.Empty;
                        }

                        // For any other value kind, continue below and use input.ToJsonString...
                    }
                    else if (value is string stringValue)
                    {
                        return stringValue ?? string.Empty;
                    }

                    // For any other type, continue below and use input.ToJsonString...
                }

                return input.ToJsonString();
            }

            return string.Empty;
        }
    }

    internal class ToStringFromStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var input = (string?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.String, input);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string?>)arguments[0].Column;

            var data = new string?[column.RowCount];
            for (var i = 0; i < column.RowCount; i++)
            {
                data[i] = column[i];
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }
    }
}
