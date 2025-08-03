// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class MakeSetIfIntFunctionImpl : IAggregateImpl
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

        var set = new HashSet<int>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    set.Add(v.Value);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetIfLongFunctionImpl : IAggregateImpl
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

        var set = new HashSet<long>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    set.Add(v.Value);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetIfDoubleFunctionImpl : IAggregateImpl
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

        var set = new HashSet<double>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    set.Add(v.Value);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetIfDecimalFunctionImpl : IAggregateImpl
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

        var set = new HashSet<decimal>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    set.Add(v.Value);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetIfTimeSpanFunctionImpl : IAggregateImpl
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

        var set = new HashSet<TimeSpan>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    set.Add(v.Value);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetIfDateTimeFunctionImpl : IAggregateImpl
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

        var set = new HashSet<DateTime>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (v.HasValue)
                {
                    set.Add(v.Value);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetIfStringFunctionImpl : IAggregateImpl
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

        var set = new HashSet<string>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
        {
            if (predicatesColumn[i] == true)
            {
                var v = valuesColumn[i];
                if (!string.IsNullOrEmpty(v))
                {
                    set.Add(v);
                    if (set.Count >= maxSize)
                    {
                        break;
                    }
                }
            }
        }

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}
