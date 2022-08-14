// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class UrlDecodeFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var url = (string?)arguments[0].Value;

            return new ScalarResult(ScalarTypes.String, Impl(url));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var urlCol = (Column<string?>)arguments[0].Column;

            var data = new string?[urlCol.RowCount];
            for (int i = 0; i < urlCol.RowCount; i++)
            {
                data[i] = Impl(urlCol[i]);
            }

            return new ColumnarResult(Column.Create(ScalarTypes.String, data));
        }

        static string? Impl(string? url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            return Uri.UnescapeDataString(url);
        }
    }
}
