// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class LogicalAndOperatorImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            return new ScalarResult(ScalarTypes.Bool, (bool)arguments[0].Value && (bool)arguments[1].Value);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
            var left = (Column<bool>)(arguments[0].Column);
            var right = (Column<bool>)(arguments[1].Column);

            var data = new bool[left.RowCount];
            for (int i = 0; i < left.RowCount; i++)
            {
                data[i] = left[i] && right[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }
}
