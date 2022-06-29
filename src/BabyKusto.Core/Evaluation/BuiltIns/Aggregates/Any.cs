// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class AnyFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = arguments[0].Column;
            return new ScalarResult(column.Type, column.RowCount > 0 ? column.RawData.GetValue(0) : null);
        }
    }
}
