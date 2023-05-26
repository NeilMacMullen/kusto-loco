// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using Kusto.Language.Symbols;
using TDigest;

namespace BabyKusto.Core.Evaluation.BuiltIns.Impl
{
    internal class PercentileAggregateIntImpl : IAggregateImpl
    {
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesCol = (Column<int?>)arguments[0].Column;
            var percentilesCol = (Column<double?>)arguments[1].Column;

            var percentile = percentilesCol[0];
            if (!percentile.HasValue)
            {
                throw new InvalidOperationException($"Requested percentile must not be null.");
            }

            var digest = new MergingDigest(compression: 100);
            for (int i = 0; i < valuesCol.RowCount; i++)
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
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesCol = (Column<long?>)arguments[0].Column;
            var percentilesCol = (Column<double?>)arguments[1].Column;

            var percentile = percentilesCol[0];
            if (!percentile.HasValue)
            {
                throw new InvalidOperationException($"Requested percentile must not be null.");
            }

            var digest = new MergingDigest(compression: 100);
            for (int i = 0; i < valuesCol.RowCount; i++)
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
        public ScalarResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
        {
            Debug.Assert(arguments.Length == 2);
            var valuesCol = (Column<double?>)arguments[0].Column;
            var percentilesCol = (Column<double?>)arguments[1].Column;

            var percentile = percentilesCol[0];
            if (!percentile.HasValue)
            {
                throw new InvalidOperationException($"Requested percentile must not be null.");
            }

            var digest = new MergingDigest(compression: 100);
            for (int i = 0; i < valuesCol.RowCount; i++)
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
}
