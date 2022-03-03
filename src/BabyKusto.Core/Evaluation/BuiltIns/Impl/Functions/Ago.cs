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
            var ago = (TimeSpan)arguments[0].Value;
            return new ScalarResult(ScalarTypes.DateTime, DateTime.UtcNow - ago);
        }

        public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var ago = (Column<TimeSpan>)arguments[0].Column;

            var data = new DateTime[ago.RowCount];
            var now = DateTime.UtcNow;
            for (int i = 0; i < ago.RowCount; i++)
            {
                data[i] = now - ago[i];
            }
            return new ColumnarResult(Column.Create(ScalarTypes.DateTime, data));
        }
    }
}
