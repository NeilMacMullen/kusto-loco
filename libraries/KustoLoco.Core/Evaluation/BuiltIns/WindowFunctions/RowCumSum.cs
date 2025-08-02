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
        var termCol = (GenericTypedBaseColumnOfint)thisWindowArguments[0].Column;
        var restartCol = (GenericTypedBaseColumnOfbool)thisWindowArguments[1].Column;

        var accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (GenericTypedBaseColumnOfint)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = NullableSetBuilderOfint.CreateFixed(termCol.RowCount);
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

            data.Add(accumulator);
        }

        return new ColumnarResult(new GenericInMemoryColumnOfint(data.ToNullableSet()));
    }
}

internal class RowCumSumLongFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (GenericTypedBaseColumnOflong)thisWindowArguments[0].Column;
        var restartCol = (GenericTypedBaseColumnOfbool)thisWindowArguments[1].Column;

        long accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (GenericTypedBaseColumnOflong)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = NullableSetBuilderOflong.CreateFixed(termCol.RowCount);
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

            data.Add(accumulator);
        }

        return new ColumnarResult(new GenericInMemoryColumnOflong(data.ToNullableSet()));
    }
}

internal class RowCumSumDoubleFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (GenericTypedBaseColumnOfdouble)thisWindowArguments[0].Column;
        var restartCol = (GenericTypedBaseColumnOfbool)thisWindowArguments[1].Column;

        double accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (GenericTypedBaseColumnOfdouble)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = NullableSetBuilderOfdouble.CreateFixed(termCol.RowCount);
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

            data.Add(accumulator);
        }

        return new ColumnarResult(new GenericInMemoryColumnOfdouble(data.ToNullableSet()));
    }
}

internal class RowCumSumDecimalFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (GenericTypedBaseColumnOfdecimal)thisWindowArguments[0].Column;
        var restartCol = (GenericTypedBaseColumnOfbool)thisWindowArguments[1].Column;

        decimal accumulator = 0;
        if (previousWindowResult != null)
        {
            var previousColumn = (GenericTypedBaseColumnOfdecimal)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = NullableSetBuilderOfdecimal.CreateFixed(termCol.RowCount);
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

            data.Add(accumulator);
        }

        return new ColumnarResult(new GenericInMemoryColumnOfdecimal(data.ToNullableSet()));
    }
}

internal class RowCumSumTimeSpanFunctionImpl : IWindowFunctionImpl
{
    public ColumnarResult InvokeWindow(ColumnarResult[] thisWindowArguments, ColumnarResult[]? previousWindowArguments,
        ColumnarResult? previousWindowResult)
    {
        Debug.Assert(thisWindowArguments.Length == 2);
        var termCol = (GenericTypedBaseColumnOfTimeSpan)thisWindowArguments[0].Column;
        var restartCol = (GenericTypedBaseColumnOfbool)thisWindowArguments[1].Column;

        var accumulator = TimeSpan.Zero;
        if (previousWindowResult != null)
        {
            var previousColumn = (GenericTypedBaseColumnOfTimeSpan)previousWindowResult.Column;
            Debug.Assert(previousColumn.RowCount > 0);

            var lastValue = previousColumn[previousColumn.RowCount - 1];
            Debug.Assert(lastValue.HasValue);

            accumulator = lastValue.Value;
        }

        var data = NullableSetBuilderOfTimeSpan.CreateFixed(termCol.RowCount);
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

            data.Add(accumulator);
        }

        return new ColumnarResult(new GenericInMemoryColumnOfTimeSpan(data.ToNullableSet()));
    }
}
