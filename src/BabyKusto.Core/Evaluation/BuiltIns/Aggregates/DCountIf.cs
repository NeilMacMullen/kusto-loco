// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class DCountIfAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesColumn = (Column<int?>)arguments[0].Column;
            var predicatesColumn = (Column<bool?>)arguments[1].Column;
            return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
        }
    }

    internal class DCountIfAggregateLongImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesColumn = (Column<long?>)arguments[0].Column;
            var predicatesColumn = (Column<bool?>)arguments[1].Column;
            return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
        }
    }

    internal class DCountIfAggregateDoubleImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesColumn = (Column<double?>)arguments[0].Column;
            var predicatesColumn = (Column<bool?>)arguments[1].Column;
            return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
        }
    }

    internal class DCountIfAggregateDateTimeImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesColumn = (Column<DateTime?>)arguments[0].Column;
            var predicatesColumn = (Column<bool?>)arguments[1].Column;
            return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
        }
    }

    internal class DCountIfAggregateTimeSpanImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesColumn = (Column<TimeSpan?>)arguments[0].Column;
            var predicatesColumn = (Column<bool?>)arguments[1].Column;
            return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
        }
    }

    internal class DCountIfAggregateStringImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesColumn = (Column<string?>)arguments[0].Column;
            var predicatesColumn = (Column<bool?>)arguments[1].Column;
            return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
        }
    }

    internal static class DCountIfHelper
    {
        public static long Compute<T>(Column<T?> values, Column<bool?> predicates)
            where T : struct
        {
            // TODO: Use HLL like real Kusto
            var seen = new HashSet<T>();
            for (int i = 0; i < values.RowCount; i++)
            {
                if (predicates[i] == true)
                {
                    var v = values[i];
                    if (v.HasValue)
                    {
                        seen.Add(v.Value);
                    }
                }
            }

            return seen.Count;
        }

        public static long Compute(Column<string?> column, Column<bool?> predicates)
        {
            // TODO: Use HLL like real Kusto
            var seen = new HashSet<string>();
            for (int i = 0; i < column.RowCount; i++)
            {
                if (predicates[i] == true)
                {
                    seen.Add(column[i] ?? string.Empty);
                }
            }

            return seen.Count;
        }
    }
}
