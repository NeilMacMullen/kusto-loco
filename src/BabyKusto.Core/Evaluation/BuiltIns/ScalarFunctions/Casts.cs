// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class ToIntStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var stringValue = (string?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.Int, Impl(stringValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string?>)arguments[0].Column;

            var data = new int?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Int, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int? Impl(string? input)
        {
            return int.TryParse(input, out var parsedResult)
                ? parsedResult
                : (double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) && !double.IsInfinity(parsedDouble))
                    ? (int)parsedDouble
                    : null;
        }
    }

    internal class ToLongStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var stringValue = (string?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.Long, Impl(stringValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string?>)arguments[0].Column;

            var data = new long?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long? Impl(string? input)
        {
            return long.TryParse(input, out var parsedResult)
                ? parsedResult
                : (double.TryParse(input, out var parsedDouble) && !double.IsNaN(parsedDouble) && !double.IsInfinity(parsedDouble))
                    ? (long)parsedDouble
                    : null;
        }
    }

    internal class ToDoubleStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var stringValue = (string?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.Real, Impl(stringValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string?>)arguments[0].Column;

            var data = new double?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Real, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double? Impl(string? input)
        {
            return double.TryParse(input, out var parsedResult) ? parsedResult : null;
        }
    }

    internal class ToBoolStringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var stringValue = (string?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.Bool, Impl(stringValue));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string?>)arguments[0].Column;

            var data = new bool?[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = Impl(column[i]);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool? Impl(string? input)
        {
            return bool.TryParse(input, out var parsedResult)
                ? parsedResult
                : long.TryParse(input, out var parsedLong)
                    ? parsedLong != 0
                    : null;
        }
    }
}
