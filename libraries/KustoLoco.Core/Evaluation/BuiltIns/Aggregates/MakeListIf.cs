// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MakeListIfIntFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfint)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<int>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListIfLongFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOflong)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<long>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListIfDoubleFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfdouble)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<double>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}


internal class MakeListIfDecimalFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfdecimal)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<decimal>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListIfTimeSpanFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfTimeSpan)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<TimeSpan>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListIfDateTimeFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfDateTime)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<DateTime>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}

internal class MakeListIfStringFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfstring)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<string>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
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
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(list));
    }
}


internal class MakeListIfDynamicFunctionImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (GenericTypedBaseColumnOfJsonNode)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        MyDebug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (GenericTypedBaseColumnOflong)arguments[2].Column;
            MyDebug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var list = new List<JsonNode?>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] != true) continue;

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
