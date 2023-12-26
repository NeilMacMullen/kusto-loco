// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class MakeListIfIntFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (TypedBaseColumn<int?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        Debug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[2].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

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
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (TypedBaseColumn<long?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        Debug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[2].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

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
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (TypedBaseColumn<double?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        Debug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[2].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

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

internal class MakeListIfTimeSpanFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (TypedBaseColumn<TimeSpan?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        Debug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[2].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

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
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (TypedBaseColumn<DateTime?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        Debug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[2].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

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
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2 || arguments.Length == 3);
        var valuesColumn = (TypedBaseColumn<string?>)arguments[0].Column;
        var predicatesColumn = (TypedBaseColumn<bool?>)arguments[1].Column;
        Debug.Assert(valuesColumn.RowCount == predicatesColumn.RowCount);

        var maxSize = long.MaxValue;
        if (arguments.Length == 3)
        {
            var maxSizeColumn = (TypedBaseColumn<long?>)arguments[2].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

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