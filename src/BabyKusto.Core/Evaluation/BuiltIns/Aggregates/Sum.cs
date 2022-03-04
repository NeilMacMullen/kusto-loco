// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class SumAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int?>)arguments[0].Column;
            int sum = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    sum += item.Value;
                }
            }
            return new ScalarResult(ScalarTypes.Int, sum);
        }
    }

    internal class SumAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long?>)arguments[0].Column;
            long sum = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    sum += item.Value;
                }
            }
            return new ScalarResult(ScalarTypes.Long, sum);
        }
    }

    internal class SumAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double?>)arguments[0].Column;
            double sum = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    sum += item.Value;
                }
            }
            return new ScalarResult(ScalarTypes.Real, sum);
        }
    }
}
