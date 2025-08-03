//
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using TDigestNet;


namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class PercentileAggregateIntImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2);
        var valuesCol = (GenericTypedBaseColumnOfint)arguments[0].Column;
        var percentilesCol = (GenericTypedBaseColumnOfdouble)arguments[1].Column;

        var percentile = percentilesCol[0];
        if (!percentile.HasValue)
        {
            throw new InvalidOperationException("Requested percentile must not be null.");
        }

        var digest = new TDigest(compression: 100);
        for (var i = 0; i < valuesCol.RowCount; i++)
        {
            var item = valuesCol[i];
            if (item.HasValue)
            {
                digest.Add(item.Value);
            }
        }

        return new ScalarResult(ScalarTypes.Long, (long)Math.Round(digest.Quantile(percentile.Value / 100.0)));
    }
}

internal class PercentileAggregateLongImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2);
        var valuesCol = (GenericTypedBaseColumnOflong)arguments[0].Column;
        var percentilesCol = (GenericTypedBaseColumnOfdouble)arguments[1].Column;

        var percentile = percentilesCol[0];
        if (!percentile.HasValue)
        {
            throw new InvalidOperationException("Requested percentile must not be null.");
        }

        var digest = new TDigest(compression: 100);
        for (var i = 0; i < valuesCol.RowCount; i++)
        {
            var item = valuesCol[i];
            if (item.HasValue)
            {
                digest.Add(item.Value);
            }
        }

        return new ScalarResult(ScalarTypes.Long, (long)Math.Round(digest.Quantile(percentile.Value / 100.0)));
    }
}

internal class PercentileAggregateDoubleImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 2);
        var valuesCol = (GenericTypedBaseColumnOfdouble)arguments[0].Column;
        var percentilesCol = (GenericTypedBaseColumnOfdouble)arguments[1].Column;

        var percentile = percentilesCol[0];
        if (!percentile.HasValue)
        {
            throw new InvalidOperationException("Requested percentile must not be null.");
        }

        var digest = new TDigest(compression: 100);
        for (var i = 0; i < valuesCol.RowCount; i++)
        {
            var item = valuesCol[i];
            if (item.HasValue)
            {
                digest.Add(item.Value);
            }
        }

        return new ScalarResult(ScalarTypes.Real, digest.Quantile(percentile.Value / 100.0));
    }
}
