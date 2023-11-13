// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DCountAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<int?>)arguments[0].Column;
            return new ScalarResult(ScalarTypes.Long, DCountHelper.Compute(column));
        }
    }

    internal class DCountAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<long?>)arguments[0].Column;
            return new ScalarResult(ScalarTypes.Long, DCountHelper.Compute(column));
        }
    }

    internal class DCountAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<double?>)arguments[0].Column;
            return new ScalarResult(ScalarTypes.Long, DCountHelper.Compute(column));
        }
    }

    internal class DCountAggregateDateTimeImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<DateTime?>)arguments[0].Column;
            return new ScalarResult(ScalarTypes.Long, DCountHelper.Compute(column));
        }
    }

    internal class DCountAggregateTimeSpanImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<TimeSpan?>)arguments[0].Column;
            return new ScalarResult(ScalarTypes.Long, DCountHelper.Compute(column));
        }
    }

    internal class DCountAggregateStringImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 1);
            var column = (Column<string?>)arguments[0].Column;
            return new ScalarResult(ScalarTypes.Long, DCountHelper.Compute(column));
        }
    }

    internal static class DCountHelper
    {
        public static long Compute<T>(Column<T?> column)
            where T : struct
        {
            // TODO: Use HLL like real Kusto
            var seen = new HashSet<T>();
            for (var i = 0; i < column.RowCount; i++)
            {
                var v = column[i];
                if (v.HasValue)
                {
                    seen.Add(v.Value);
                }
            }

            return seen.Count;
        }

        public static long Compute(Column<string?> column)
        {
            // TODO: Use HLL like real Kusto
            var seen = new HashSet<string>();
            for (var i = 0; i < column.RowCount; i++)
            {
                seen.Add(column[i] ?? string.Empty);
            }

            return seen.Count;
        }
    }
}
