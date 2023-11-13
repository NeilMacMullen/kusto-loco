// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class IsEmptyFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var value = (string?)arguments[0].Value;

            return new ScalarResult(ScalarTypes.Bool, string.IsNullOrEmpty(value));
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valueCol = (Column<string?>)(arguments[0].Column);

            var data = new bool?[valueCol.RowCount];
            for (var i = 0; i < valueCol.RowCount; i++)
            {
                var value = valueCol[i];
                data[i] = string.IsNullOrEmpty(value);
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }
}
