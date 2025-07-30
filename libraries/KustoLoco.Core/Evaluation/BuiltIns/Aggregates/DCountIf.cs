// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class DCountIfAggregateIntImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfint)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOfint.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateLongImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOflong)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOflong.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateDoubleImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfdouble)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOfdouble.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateDecimalImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfdecimal)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOfdecimal.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateDateTimeImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfDateTime)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOfDateTime.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateTimeSpanImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfTimeSpan)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOfTimeSpan.Compute(valuesColumn, predicatesColumn));
    }
}

internal class DCountIfAggregateStringImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        Debug.Assert(arguments.Length == 2);
        var valuesColumn = (GenericTypedBaseColumnOfstring)arguments[0].Column;
        var predicatesColumn = (GenericTypedBaseColumnOfbool)arguments[1].Column;
        return new ScalarResult(ScalarTypes.Long, DCountIfHelperOfstring.Compute(valuesColumn, predicatesColumn));
    }
}

[KustoGeneric(Types = "all")]
internal static class DCountIfHelper<T>
{
    public static long Compute(GenericTypedBaseColumn<T> values, GenericTypedBaseColumnOfbool predicates)
    {
        // TODO: Use HLL like real Kusto
        var seen = new HashSet<T?>(); //GENERIC INLINE
        for (var i = 0; i < values.RowCount; i++)
        {
            if (predicates[i] == true)
            {
                var v = values[i];
#if TYPE_STRING
                seen.Add(v ?? string.Empty);
#else
                if (v!=null)
                {
                    seen.Add(v);
                }
#endif
            }
        }

        return seen.Count;
    }

}
