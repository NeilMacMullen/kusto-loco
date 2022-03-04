// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class MaxAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int?>)arguments[0].Column;
            int? max = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (max == null || v > max.Value)
                {
                    max = v;
                }
            }

            return new ScalarResult(ScalarTypes.Int, max);
        }
    }

    internal class MaxAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long?>)arguments[0].Column;
            long? max = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (max == null || v > max.Value)
                {
                    max = v;
                }
            }
            return new ScalarResult(ScalarTypes.Long, max);
        }
    }

    internal class MaxAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double?>)arguments[0].Column;
            double? max = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (max == null || v > max.Value)
                {
                    max = v;
                }
            }
            return new ScalarResult(ScalarTypes.Real, max);
        }
    }

    internal class MaxAggregateDateTimeImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<DateTime?>)arguments[0].Column;
            DateTime? max = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (max == null || v > max.Value)
                {
                    max = v;
                }
            }
            return new ScalarResult(ScalarTypes.DateTime, max);
        }
    }

    internal class MaxAggregateTimeSpanImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<TimeSpan?>)arguments[0].Column;
            TimeSpan? max = null;
            for (int i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (max == null || v > max.Value)
                {
                    max = v;
                }
            }
            return new ScalarResult(ScalarTypes.TimeSpan, max);
        }
    }
}
