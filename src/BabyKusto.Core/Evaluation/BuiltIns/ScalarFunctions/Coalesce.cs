// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class CoalesceBoolFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        bool? result = null;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (bool?)arguments[i].Value;
            if (item.HasValue)
            {
                result = item.Value;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.Bool, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new bool?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<bool?>)arguments[i].Column;
                var item = column[j];
                if (item.HasValue)
                {
                    data[j] = item.Value;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class CoalesceIntFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        int? result = null;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (int?)arguments[i].Value;
            if (item.HasValue)
            {
                result = item.Value;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.Int, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new int?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<int?>)arguments[i].Column;
                var item = column[j];
                if (item.HasValue)
                {
                    data[j] = item.Value;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class CoalesceLongFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        long? result = null;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (long?)arguments[i].Value;
            if (item.HasValue)
            {
                result = item.Value;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.Long, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new long?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<long?>)arguments[i].Column;
                var item = column[j];
                if (item.HasValue)
                {
                    data[j] = item.Value;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class CoalesceDoubleFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        double? result = null;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (double?)arguments[i].Value;
            if (item.HasValue)
            {
                result = item.Value;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.Real, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new double?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<double?>)arguments[i].Column;
                var item = column[j];
                if (item.HasValue)
                {
                    data[j] = item.Value;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class CoalesceDateTimeFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        DateTime? result = null;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (DateTime?)arguments[i].Value;
            if (item.HasValue)
            {
                result = item.Value;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.DateTime, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new DateTime?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<DateTime?>)arguments[i].Column;
                var item = column[j];
                if (item.HasValue)
                {
                    data[j] = item.Value;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class CoalesceTimeSpanFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        TimeSpan? result = null;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (TimeSpan?)arguments[i].Value;
            if (item.HasValue)
            {
                result = item.Value;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.TimeSpan, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new TimeSpan?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<TimeSpan?>)arguments[i].Column;
                var item = column[j];
                if (item.HasValue)
                {
                    data[j] = item.Value;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}

internal class CoalesceStringFunctionImpl : IScalarFunctionImpl
{
    public ScalarResult InvokeScalar(ScalarResult[] arguments)
    {
        var result = string.Empty;
        for (var i = 0; i < arguments.Length; i++)
        {
            var item = (string?)arguments[i].Value;
            if (!string.IsNullOrEmpty(item))
            {
                result = item;
                break;
            }
        }

        return new ScalarResult(ScalarTypes.String, result);
    }

    public ColumnarResult InvokeColumnar(ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length > 0);

        var numRows = arguments[0].Column.RowCount;
        var data = new string?[numRows];
        for (var j = 0; j < numRows; j++)
        {
            for (var i = 0; i < arguments.Length; i++)
            {
                var column = (TypedBaseColumn<string?>)arguments[i].Column;
                var item = column[j];
                if (!string.IsNullOrEmpty(item))
                {
                    data[j] = item;
                    break;
                }
            }
        }

        return new ColumnarResult(ColumnFactory.Create(data));
    }
}