// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DCountIfAggregateIntImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<int?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateLongImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<long?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateDoubleImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<double?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateDateTimeImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<DateTime?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateTimeSpanImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<TimeSpan?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateStringImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<string?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelper.Compute(valuesColumn, predicatesColumn));
    }
}

internal static class DCountIfHelper
{
    public static long Compute<T>(TypedBaseColumn<T?> values, TypedBaseColumn<bool?> predicates)
        where T : struct
    {
        // TODO: Use HLL like real Kusto
        var seen = new HashSet<T>();
        for (var i = 0; i < values.RowCount; i++)
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

    public static long Compute(TypedBaseColumn<string?> column, TypedBaseColumn<bool?> predicates)
    {
        // TODO: Use HLL like real Kusto
        var seen = new HashSet<string>();
        for (var i = 0; i < column.RowCount; i++)
        {
            if (predicates[i] == true)
            {
                seen.Add(column[i] ?? string.Empty);
            }
        }

        return seen.Count;
    }
}