// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class RowCumSumIntFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (TypedBaseColumn<int?>)thisWindowArguments[0].Column;
        var restartCol = (TypedBaseColumn<bool?>)thisWindowArguments[1].Column;

        var accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (TypedBaseColumn<int?>)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = new int?[termCol.RowCount];
        for (var i = 0; i < termCol.RowCount; i++)
        {
            var restart = restartCol[i];
            if (restart == true)
            {
                accumulator = 0;
            }

            var term = termCol[i];
            if (term.HasValue)
            {
                accumulator += term.Value;
            }

            data[i] = accumulator;
        }

        return new ColumnarResult(new InMemoryColumn<int?>(data));
    }
}

internal class RowCumSumLongFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (TypedBaseColumn<long?>)thisWindowArguments[0].Column;
        var restartCol = (TypedBaseColumn<bool?>)thisWindowArguments[1].Column;

        long accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (TypedBaseColumn<long?>)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = new long?[termCol.RowCount];
        for (var i = 0; i < termCol.RowCount; i++)
        {
            var restart = restartCol[i];
            if (restart == true)
            {
                accumulator = 0;
            }

            var term = termCol[i];
            if (term.HasValue)
            {
                accumulator += term.Value;
            }

            data[i] = accumulator;
        }

        return new ColumnarResult(new InMemoryColumn<long?>(data));
    }
}

internal class RowCumSumDoubleFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (TypedBaseColumn<double?>)thisWindowArguments[0].Column;
        var restartCol = (TypedBaseColumn<bool?>)thisWindowArguments[1].Column;

        double accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (TypedBaseColumn<double?>)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = new double?[termCol.RowCount];
        for (var i = 0; i < termCol.RowCount; i++)
        {
            var restart = restartCol[i];
            if (restart == true)
            {
                accumulator = 0;
            }

            var term = termCol[i];
            if (term.HasValue)
            {
                accumulator += term.Value;
            }

            data[i] = accumulator;
        }

        return new ColumnarResult(new InMemoryColumn<double?>(data));
    }
}

internal class RowCumSumTimeSpanFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (TypedBaseColumn<TimeSpan?>)thisWindowArguments[0].Column;
        var restartCol = (TypedBaseColumn<bool?>)thisWindowArguments[1].Column;

        var accumulator = TimeSpan.Zero;
        if (previousWindowResult != null)
        {
            var previousColumn = (TypedBaseColumn<TimeSpan?>)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = new TimeSpan?[termCol.RowCount];
        for (var i = 0; i < termCol.RowCount; i++)
        {
            var restart = restartCol[i];
            if (restart == true)
            {
                accumulator = TimeSpan.Zero;
            }

            var term = termCol[i];
            if (term.HasValue)
            {
                accumulator += term.Value;
            }

            data[i] = accumulator;
        }

        return new ColumnarResult(new InMemoryColumn<TimeSpan?>(data));
    }
}