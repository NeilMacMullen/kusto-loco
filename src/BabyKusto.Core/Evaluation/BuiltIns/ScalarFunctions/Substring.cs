// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class SubstringFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var input = (string?)arguments[0].Value;
            var start = (long?)arguments[1].Value;
            var length = (long?)arguments[2].Value;

            return new ScalarResult(ScalarTypes.String, Impl(input, start, length));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 3);
            var inputCol = (Column<string?>)arguments[0].Column;
            var startCol = (Column<long?>)arguments[1].Column;
            var lengthCol = (Column<long?>)arguments[2].Column;

            var data = new string?[inputCol.RowCount];
            for (var i = 0; i < inputCol.RowCount; i++)
            {
                data[i] = Impl(inputCol[i], startCol[i], lengthCol[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        static string? Impl(string? input, long? start, long? length)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var effectiveStart = start.HasValue ? Math.Max(0, Math.Min(start.Value, input.Length)) : 0;
            var maxAllowableLength = input.Length - effectiveStart;
            var effectiveLength = length.HasValue ? Math.Max(0, Math.Min(length.Value, maxAllowableLength)) : maxAllowableLength;
            return input.Substring((int)effectiveStart, (int)effectiveLength);
        }
    }
}
