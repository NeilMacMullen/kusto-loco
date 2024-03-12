// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MakeListIntFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<int?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<int>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v.HasValue)
            {
                list.Add(v.Value);
                if (list.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListLongFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<long?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<long>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v.HasValue)
            {
                list.Add(v.Value);
                if (list.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListDoubleFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<double?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<double>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v.HasValue)
            {
                list.Add(v.Value);
                if (list.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListTimeSpanFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<TimeSpan?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<TimeSpan>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v.HasValue)
            {
                list.Add(v.Value);
                if (list.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListDateTimeFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<DateTime?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<DateTime>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v.HasValue)
            {
                list.Add(v.Value);
                if (list.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListStringFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<string?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<string>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (!string.IsNullOrEmpty(v))
            {
                list.Add(v);
                if (list.Count >= maxSize)
                {
                    break;
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}