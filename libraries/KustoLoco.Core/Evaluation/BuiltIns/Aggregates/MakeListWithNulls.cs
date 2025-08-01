﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MakeListWithNullsIntFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<int?>)arguments[0].Column;

        var list = new List<int?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListWithNullsLongFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<long?>)arguments[0].Column;

        var list = new List<long?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListWithNullsDoubleFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<double?>)arguments[0].Column;

        var list = new List<double?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListWithNullsDecimalFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<decimal?>)arguments[0].Column;

        var list = new List<decimal?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListWithNullsTimeSpanFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<TimeSpan?>)arguments[0].Column;

        var list = new List<TimeSpan?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListWithNullsDateTimeFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<DateTime?>)arguments[0].Column;

        var list = new List<DateTime?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListWithNullsStringFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var valuesColumn = (TypedBaseColumn<string?>)arguments[0].Column;

        var list = new List<string?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            list.Add(valuesColumn[i]);
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}
