// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class AvgAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int>)arguments[0].Column;
            double sum = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                sum += column[i];
            }
            return new ScalarResult(ScalarTypes.Real, sum / chunk.RowCount);
        }
    }

    internal class AvgAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long>)arguments[0].Column;
            double sum = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                sum += column[i];
            }
            return new ScalarResult(ScalarTypes.Real, sum / chunk.RowCount);
        }
    }

    internal class AvgAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double>)arguments[0].Column;
            double sum = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                sum += column[i];
            }
            return new ScalarResult(ScalarTypes.Real, sum / chunk.RowCount);
        }
    }
}
