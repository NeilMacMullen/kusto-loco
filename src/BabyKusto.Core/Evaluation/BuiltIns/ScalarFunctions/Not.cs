// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class NotFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var value = (bool?)arguments[0].Value;

            return new ScalarResult(ScalarTypes.Bool, value.HasValue ? !value.Value : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var valueCol = (Column<bool?>)(arguments[0].Column);

            var data = new bool?[valueCol.RowCount];
            for (int i = 0; i < valueCol.RowCount; i++)
            {
                var value = valueCol[i];
                data[i] = value.HasValue ? !value.Value : null;
            }

            return new ColumnarResult(Column.Create(ScalarTypes.Bool, data));
        }
    }
}
