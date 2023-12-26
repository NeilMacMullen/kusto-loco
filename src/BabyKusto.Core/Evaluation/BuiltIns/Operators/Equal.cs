// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class EqualIntOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        return new ScalarResult(ScalarTypes.Bool, (int?)arguments[0].Value == (int?)arguments[1].Value);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<int?>)(arguments[0].Column);
        var right = (TypedBaseColumn<int?>)(arguments[1].Column);

        var data = new bool?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = left[i] == right[i];
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class EqualLongOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        return new ScalarResult(ScalarTypes.Bool, (long?)arguments[0].Value == (long?)arguments[1].Value);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<long?>)(arguments[0].Column);
        var right = (TypedBaseColumn<long?>)(arguments[1].Column);

        var data = new bool?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = left[i] == right[i];
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class EqualDoubleOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        return new ScalarResult(ScalarTypes.Bool, (double?)arguments[0].Value == (double?)arguments[1].Value);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<double?>)(arguments[0].Column);
        var right = (TypedBaseColumn<double?>)(arguments[1].Column);

        var data = new bool?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = left[i] == right[i];
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class EqualStringOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        return new ScalarResult(ScalarTypes.Bool,
            ((string?)arguments[0].Value ?? string.Empty) == ((string?)arguments[1].Value ?? string.Empty));
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<string?>)(arguments[0].Column);
        var right = (TypedBaseColumn<string?>)(arguments[1].Column);

        var data = new bool?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = string.Equals(left[i] ?? string.Empty, right[i] ?? string.Empty, StringComparison.Ordinal);
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class EqualTimeSpanOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        return new ScalarResult(ScalarTypes.Bool, (TimeSpan?)arguments[0].Value == (TimeSpan?)arguments[1].Value);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<TimeSpan?>)(arguments[0].Column);
        var right = (TypedBaseColumn<TimeSpan?>)(arguments[1].Column);

        var data = new bool?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = left[i] == right[i];
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}

internal class EqualDateTimeOperatorImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        return new ScalarResult(ScalarTypes.Bool, (DateTime?)arguments[0].Value == (DateTime?)arguments[1].Value);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        Debug.Assert(arguments[0].Column.RowCount == arguments[1].Column.RowCount);
        var left = (TypedBaseColumn<DateTime?>)(arguments[0].Column);
        var right = (TypedBaseColumn<DateTime?>)(arguments[1].Column);

        var data = new bool?[left.RowCount];
        for (var i = 0; i < left.RowCount; i++)
        {
            data[i] = left[i] == right[i];
        }

        return new ColumnarResult(ColumnFactory.Create(ScalarTypes.Bool, data));
    }
}