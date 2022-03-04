// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class MinAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int>)arguments[0].Column;
            int? min = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (min == null || v < min.Value)
                {
                    min = v;
                }
            }

            return new ScalarResult(ScalarTypes.Int, min);
        }
    }

    internal class MinAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long>)arguments[0].Column;
            long? min = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (min == null || v < min.Value)
                {
                    min = v;
                }
            }
            return new ScalarResult(ScalarTypes.Long, min);
        }
    }

    internal class MinAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double>)arguments[0].Column;
            double? min = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (min == null || v < min.Value)
                {
                    min = v;
                }
            }
            return new ScalarResult(ScalarTypes.Real, min);
        }
    }

    internal class MinAggregateDateTimeImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<DateTime>)arguments[0].Column;
            DateTime? min = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (min == null || v < min.Value)
                {
                    min = v;
                }
            }
            return new ScalarResult(ScalarTypes.DateTime, min);
        }
    }

    internal class MinAggregateTimeSpanImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<TimeSpan>)arguments[0].Column;
            TimeSpan? min = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (min == null || v < min.Value)
                {
                    min = v;
                }
            }
            return new ScalarResult(ScalarTypes.TimeSpan, min);
        }
    }
}
