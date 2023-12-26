﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class LessThanOrEqualIntOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (int?)arguments[0].Value;
        var right = (int?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left <= right : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<int?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<int?>)(arguments[1].Column);

        var data = new bool?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left <= right : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class LessThanOrEqualLongOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (long?)arguments[0].Value;
        var right = (long?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left <= right : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<long?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<long?>)(arguments[1].Column);

        var data = new bool?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left <= right : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class LessThanOrEqualDoubleOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (double?)arguments[0].Value;
        var right = (double?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left <= right : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<double?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<double?>)(arguments[1].Column);

        var data = new bool?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left <= right : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class LessThanOrEqualTimeSpanOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (TimeSpan?)arguments[0].Value;
        var right = (TimeSpan?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left <= right : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<TimeSpan?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<TimeSpan?>)(arguments[1].Column);

        var data = new bool?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left <= right : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class LessThanOrEqualDateTimeOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (DateTime?)arguments[0].Value;
        var right = (DateTime?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Bool, left.HasValue && right.HasValue ? left <= right : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<DateTime?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<DateTime?>)(arguments[1].Column);

        var data = new bool?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left <= right : null;
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}