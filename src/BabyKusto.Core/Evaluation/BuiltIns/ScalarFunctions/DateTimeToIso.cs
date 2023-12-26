// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Globalization;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DateTimeToIsoImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var input = (DateTime?)arguments[0].Value;

            return new ScalarResult(ScalarTypes.String, Impl(input));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var inputCol = (TypedBaseColumn<DateTime?>)arguments[0].Column;

            var data = new string?[inputCol.RowCount];
            for (var i = 0; i < inputCol.RowCount; i++)
            {
                data[i] = Impl(inputCol[i]);
            }

            return new ColumnarResult(ColumnFactory.Create(data));
        }

        private static string? Impl(DateTime? input)
            => input?.ToString("o", CultureInfo.InvariantCulture) ?? string.Empty;
    }
}