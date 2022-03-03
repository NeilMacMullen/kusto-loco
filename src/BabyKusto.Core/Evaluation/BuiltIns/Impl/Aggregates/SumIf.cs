// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class SumIfIntFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);

            var expression = (Column<int>)arguments[0].Column;
            var predicate = (Column<bool>)arguments[1].Column;
            int sum = 0;
            for (int i = 0; i < predicate.RowCount; i++)
            {
                if (predicate[i])
                {
                    sum += expression[i];
                }
            }

            return new ScalarResult(ScalarTypes.Int, sum);
        }
    }

    internal class SumIfLongFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);

            var expression = (Column<long>)arguments[0].Column;
            var predicate = (Column<bool>)arguments[1].Column;
            long sum = 0;
            for (int i = 0; i < predicate.RowCount; i++)
            {
                if (predicate[i])
                {
                    sum += expression[i];
                }
            }

            return new ScalarResult(ScalarTypes.Long, sum);
        }
    }

    internal class SumIfDoubleFunctionImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);

            var expression = (Column<double>)arguments[0].Column;
            var predicate = (Column<bool>)arguments[1].Column;
            double sum = 0;
            for (int i = 0; i < predicate.RowCount; i++)
            {
                if (predicate[i])
                {
                    sum += expression[i];
                }
            }

            return new ScalarResult(ScalarTypes.Real, sum);
        }
    }
}
