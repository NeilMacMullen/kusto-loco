// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl;

internal class MakeSetIntFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (Column<int?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (Column<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<int>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
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

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetLongFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (Column<long?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (Column<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<long>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
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

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetDoubleFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (Column<double?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (Column<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<double>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
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

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetTimeSpanFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (Column<TimeSpan?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (Column<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<TimeSpan>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
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

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetDateTimeFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (Column<DateTime?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (Column<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<DateTime>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
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

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}

internal class MakeSetStringFunctionImpl : IAggregateImpl
{
    public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1 || arguments.Length == 2);
        var valuesColumn = (Column<string?>)arguments[0].Column;

        var maxSize = long.MaxValue;
        if (arguments.Length == 2)
        {
            var maxSizeColumn = (Column<long?>)arguments[1].Column;
            Debug.Assert(valuesColumn.RowCount == maxSizeColumn.RowCount);

            if (maxSizeColumn.RowCount > 0)
            {
                maxSize = maxSizeColumn[0] ?? long.MaxValue;
            }
        }

        var set = new HashSet<string>();
        for (var i = 0; i < valuesColumn.RowCount; i++)
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

        return new ScalarResult(ScalarTypes.Dynamic, JsonArrayHelper.From(set));
    }
}