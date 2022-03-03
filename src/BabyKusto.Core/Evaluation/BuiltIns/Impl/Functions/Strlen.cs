// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class StrlenFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var text = (string)arguments[0].Value;
            return new ScalarResult(ScalarTypes.Long, (long)(text ?? string.Empty).Length);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string>)arguments[0].Column;

            var data = new long[column.RowCount];
            for (int i = 0; i < column.RowCount; i++)
            {
                data[i] = (long)(column[i] ?? string.Empty).Length;
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Long, data));
        }
    }
}
