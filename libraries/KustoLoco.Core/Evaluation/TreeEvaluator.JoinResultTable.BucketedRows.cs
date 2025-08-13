using System.Collections.Generic;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    private partial class JoinResultTable
    {
        private class BucketedRows
        {
            public BucketedRows(ITableSource table)
            {
                Table = table;
            }

            public ITableSource Table { get; }
            public Dictionary<SummaryKey, JoinSet> Buckets { get; } = new();
        }
    }
}
