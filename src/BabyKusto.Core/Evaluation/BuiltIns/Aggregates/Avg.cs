// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

public class AvgAcc
{
    public int Count;
    public double Total;
}

internal class AvgAggregateIntImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<int?>)arguments[0].Column;
        double sum = 0;
        var count = 0;
        for (var i = 0; i < column.RowCount; i++)
        {
            var item = column[i];
            if (item.HasValue)
            {
                sum += item.Value;
                count++;
            }
        }

        return new ScalarResult(ScalarTypes.Real, count == 0
            ? null
            : sum / count);
    }
}

internal class AvgAggregateLongImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<long?>)arguments[0].Column;
        double sum = 0;
        var count = 0;
        for (var i = 0; i < column.RowCount; i++)
        {
            var item = column[i];
            if (item.HasValue)
            {
                sum += item.Value;
                count++;
            }
        }

        return new ScalarResult(ScalarTypes.Real, count == 0
            ? null
            : sum / count);
    }
}

internal class AvgAggregateDoubleImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<double?>)arguments[0].Column;
        double sum = 0;
        var count = 0;
        for (var i = 0; i < column.RowCount; i++)
        {
            var item = column[i];
            if (item.HasValue)
            {
                sum += item.Value;
                count++;
            }
        }

        return new ScalarResult(ScalarTypes.Real, count == 0
            ? null
            : sum / count);
    }
}

internal class AvgAggregateTimeSpanImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (TypedBaseColumn<TimeSpan?>)arguments[0].Column;
        double sum = 0;
        var count = 0;
        for (var i = 0; i < column.RowCount; i++)
        {
            var item = column[i];
            if (item.HasValue)
            {
                sum += item.Value.Ticks;
                count++;
            }
        }

        return new ScalarResult(ScalarTypes.TimeSpan, count == 0
            ? null
            : new TimeSpan((long)(sum / count)));
    }
}