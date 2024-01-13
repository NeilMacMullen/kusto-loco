// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class IsNullBoolFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (bool?)arguments[0].Value;

        return new ScalarResult(ScalarTypes.Bool, value == null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valueCol = (TypedBaseColumn<bool?>)(arguments[0].Column);

        var data = new bool?[valueCol.RowCount];
        for (var i = 0; i < valueCol.RowCount; i++)
        {
            var value = valueCol[i];
            data[i] = value == null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class IsNullIntFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (int?)arguments[0].Value;

        return new ScalarResult(ScalarTypes.Bool, value == null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valueCol = (TypedBaseColumn<int?>)(arguments[0].Column);

        var data = new bool?[valueCol.RowCount];
        for (var i = 0; i < valueCol.RowCount; i++)
        {
            var value = valueCol[i];
            data[i] = value == null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class IsNullLongFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (long?)arguments[0].Value;

        return new ScalarResult(ScalarTypes.Bool, value == null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valueCol = (TypedBaseColumn<long?>)(arguments[0].Column);

        var data = new bool?[valueCol.RowCount];
        for (var i = 0; i < valueCol.RowCount; i++)
        {
            var value = valueCol[i];
            data[i] = value == null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class IsNullDoubleFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (double?)arguments[0].Value;

        return new ScalarResult(ScalarTypes.Bool, value == null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valueCol = (TypedBaseColumn<double?>)(arguments[0].Column);

        var data = new bool?[valueCol.RowCount];
        for (var i = 0; i < valueCol.RowCount; i++)
        {
            var value = valueCol[i];
            data[i] = value == null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class IsNullDateTimeFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (DateTime?)arguments[0].Value;

        return new ScalarResult(ScalarTypes.Bool, value == null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valueCol = (TypedBaseColumn<DateTime?>)(arguments[0].Column);

        var data = new bool?[valueCol.RowCount];
        for (var i = 0; i < valueCol.RowCount; i++)
        {
            var value = valueCol[i];
            data[i] = value == null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class IsNullTimeSpanFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var value = (TimeSpan?)arguments[0].Value;

        return new ScalarResult(ScalarTypes.Bool, value == null);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valueCol = (TypedBaseColumn<TimeSpan?>)(arguments[0].Column);

        var data = new bool?[valueCol.RowCount];
        for (var i = 0; i < valueCol.RowCount; i++)
        {
            var value = valueCol[i];
            data[i] = value == null;
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }

    internal static bool Impl(string s) => false;
}