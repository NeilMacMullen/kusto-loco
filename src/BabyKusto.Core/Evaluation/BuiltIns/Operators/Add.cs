// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class AddIntOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (int?)arguments[0].Value;
        var right = (int?)arguments[1].Value;
        return new ScalarResult(ScalarTypes.Long, left.HasValue && right.HasValue ? left.Value + right.Value : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<int?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<int?>)(arguments[1].Column);

        var data = new long?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left.Value + right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class AddLongOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (long?)arguments[0].Value;
        var right = (long?)arguments[1].Value;

        return new ScalarResult(ScalarTypes.Long, left.HasValue && right.HasValue ? left.Value + right.Value : null);
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
            data[i] = left.HasValue && right.HasValue ? left.Value + right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class AddDoubleOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (double?)arguments[0].Value;
        var right = (double?)arguments[1].Value;

        return new ScalarResult(ScalarTypes.Real, left.HasValue && right.HasValue ? left.Value + right.Value : null);
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
            data[i] = left.HasValue && right.HasValue ? left.Value + right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class AddTimeSpanOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (TimeSpan?)arguments[0].Value;
        var right = (TimeSpan?)arguments[1].Value;

        return new ScalarResult(ScalarTypes.TimeSpan,
            left.HasValue && right.HasValue ? new TimeSpan(left.Value.Ticks + right.Value.Ticks) : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<TimeSpan?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<TimeSpan?>)(arguments[1].Column);

        var data = new TimeSpan?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? new TimeSpan(left.Value.Ticks + right.Value.Ticks) : null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class AddDateTimeTimeSpanOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var left = (DateTime?)arguments[0].Value;
        var right = (TimeSpan?)arguments[1].Value;

        return new ScalarResult(ScalarTypes.DateTime,
            left.HasValue && right.HasValue ? left.Value + right.Value : null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var leftCol = (TypedBaseColumn<DateTime?>)(arguments[0].Column);
        var rightCol = (TypedBaseColumn<TimeSpan?>)(arguments[1].Column);

        var data = new DateTime?[leftCol.RowCount];
        for (var i = 0; i < leftCol.RowCount; i++)
        {
            var (left, right) = (leftCol[i], rightCol[i]);
            data[i] = left.HasValue && right.HasValue ? left.Value + right.Value : null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}