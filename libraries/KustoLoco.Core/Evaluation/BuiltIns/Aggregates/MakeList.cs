﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MakeListIntFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
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
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
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
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
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


internal class MakeListDecimalFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<decimal?>)arguments[0].Column;

        var maxSize = decimal.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (TypedBaseColumn<decimal?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? decimal.MaxValue;
            }
        }

        var list = new List<decimal>();
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
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
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
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
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
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
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

internal class MakeListDynamicFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (TypedBaseColumn<JsonNode?>)arguments[0].Column;

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

        var list = new List<JsonNode?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            var v = valuesColumn[i];
            if (v == null) continue;
            list.Add(v);
            if (list.Count >= maxSize)
            {
                break;
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}
