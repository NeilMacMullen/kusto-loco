// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class CountIfFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<bool>)arguments[0].Column;
            long count = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                if (column[i])
                {
                    count++;
                }
            }

            return new ScalarResult(ScalarTypes.Long, count);
        }
    }
}
