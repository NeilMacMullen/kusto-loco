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
            var column = (Column<int?>)arguments[0].Column;
            double sum = 0;
            int count = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    sum += item.Value;
                    count++;
                }
            }
            return new ScalarResult(ScalarTypes.Real, count == 0 ? null : sum / count);
        }
    }

    internal class AvgAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long?>)arguments[0].Column;
            double sum = 0;
            int count = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    sum += item.Value;
                    count++;
                }
            }
            return new ScalarResult(ScalarTypes.Real, count == 0 ? null : sum / count);
        }
    }

    internal class AvgAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double?>)arguments[0].Column;
            double sum = 0;
            int count = 0;
            for (int i = 0; i < column.RowCount; i++)
            {
                var item = column[i];
                if (item.HasValue)
                {
                    sum += item.Value;
                    count++;
                }
            }
            return new ScalarResult(ScalarTypes.Real, count == 0 ? null : sum / count);
        }
    }
}
