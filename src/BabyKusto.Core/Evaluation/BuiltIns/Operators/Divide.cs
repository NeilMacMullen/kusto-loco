﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class DivideIntOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (int?)arguments[0].Value;
        var right = (int?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Int,
            left.HasValue && right.HasValue && right.Value != 0 ? left.Value / right.Value : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<int?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<int?>)(arguments[1].Column);

        var data = new int?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue && right.Value != 0 ? left.Value / right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Int, data));
    }
}

internal class DivideLongOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (long?)arguments[0].Value;
        var right = (long?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Long,
            left.HasValue && right.HasValue && right.Value != 0 ? left.Value / right.Value : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<long?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<long?>)(arguments[1].Column);

        var data = new long?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue && right.Value != 0 ? left.Value / right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Long, data));
    }
}

internal class DivideDoubleOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (double?)arguments[0].Value;
        var right = (double?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Real, left.HasValue && right.HasValue ? left.Value / right.Value : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<double?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<double?>)(arguments[1].Column);

        var data = new double?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left.Value / right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Real, data));
    }
}

internal class DivideTimeSpanOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (TimeSpan?)arguments[0].Value;
        var right = (TimeSpan?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Real,
            left.HasValue && right.HasValue && right.Value.Ticks != 0
                ? (double)left.Value.Ticks / right.Value.Ticks
                : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<TimeSpan?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<TimeSpan?>)(arguments[1].Column);

        var data = new double?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue && right.Value.Ticks != 0
                ? (double)left.Value.Ticks / right.Value.Ticks
                : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Real, data));
    }
}