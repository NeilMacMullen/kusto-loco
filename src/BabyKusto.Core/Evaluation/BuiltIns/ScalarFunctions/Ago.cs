// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class AgoFunctionImpl : IScalarFunctionImpl
    {
        public ScalarResult InvokeScalar(ScalarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var ago = (TimeSpan?)arguments[0].Value;
            return new ScalarResult(ScalarTypes.DateTime, ago.HasValue ? DateTime.UtcNow - ago : null);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<TimeSpan?>)arguments[0].Column;

            var data = new DateTime?[column.RowCount];
            var now = DateTime.UtcNow;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    data[i] = now - item.Value;
                }
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }
}
