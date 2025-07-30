// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DCountAggregateIntImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfint)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOfint.Compute(column));
    }
}

internal class DCountAggregateLongImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOflong)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOflong.Compute(column));
    }
}

internal class DCountAggregateDoubleImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfdouble)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOfdouble.Compute(column));
    }
}

internal class DCountAggregateDecimalImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfdecimal)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOfdecimal.Compute(column));
    }
}

internal class DCountAggregateDateTimeImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfDateTime)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOfDateTime.Compute(column));
    }
}

internal class DCountAggregateTimeSpanImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfTimeSpan)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOfTimeSpan.Compute(column));
    }
}

internal class DCountAggregateStringImpl : IAggregateImpl
{
   public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 1);
        var column = (GenericTypedBaseColumnOfstring)arguments[0].Column;
        return new ScalarResult(ScalarTypes.Long, DCountHelperOfstring.Compute(column));
    }
}
[KustoGeneric(Types = "all")]
internal static class DCountHelper<T>
{
    public static long Compute(GenericTypedBaseColumn<T> column)
    {
        // TODO: Use HLL like real Kusto
        var seen = new HashSet<T?>(); //GENERIC INLINE

        for (var i = 0; i < column.RowCount; i++)
        {
            var v = column[i];
#if TYPE_STRING
            seen.Add(v ?? string.Empty);
#else
            if (v != null)
            {
                seen.Add(v);
            }
#endif


        }

        return seen.Count;
    }
}
