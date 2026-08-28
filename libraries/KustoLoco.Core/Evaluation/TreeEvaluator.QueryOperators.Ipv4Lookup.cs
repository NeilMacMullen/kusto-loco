//
// Licensed under the MIT License.

using System.Linq;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;
using KustoLoco.Core.InternalRepresentation.Nodes.Expressions.QueryOperators;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.Evaluation;

internal partial class TreeEvaluator
{
    public override EvaluationResult VisitIpv4LookupOperator(IRIpv4LookupOperatorNode node, EvaluationContext context)
    {
        var source = context.Left.Value;
        var sourceSchema = source.Type;
        var resultSchema = (TableSymbol)node.ResultType;

        var srcChunk = ChunkHelpers.Reassemble(source.GetData().ToArray());
        var sourceIpColumn = ((ColumnarResult)node.SourceIp.Accept(this, context with { Chunk = srcChunk })).Column;

        // Evaluate and materialise the lookup table.
        var lookup = ((TabularResult)node.LookupTable.Accept(this, context)).Value;
        var lookupSchema = lookup.Type;
        var lookupChunk = ChunkHelpers.Reassemble(lookup.GetData().ToArray());
        var lookupIpIdx = IndexOf(lookupSchema, node.LookupIpColumn);

        var outBuilders = ColumnHelpers.CreateBuildersForTable(resultSchema);

        for (var sr = 0; sr < srcChunk.RowCount; sr++)
        {
            var ip = sourceIpColumn.GetRawDataValue(sr)?.ToString();
            var matched = false;
            if (ip != null && lookupIpIdx >= 0)
                for (var lr = 0; lr < lookupChunk.RowCount; lr++)
                {
                    var cidr = lookupChunk.Columns[lookupIpIdx].GetRawDataValue(lr)?.ToString();
                    if (cidr != null && Ipv4Support.InRange(ip, cidr) == true)
                    {
                        AppendJoinedRow(outBuilders, resultSchema, sourceSchema, lookupSchema, srcChunk, sr, lookupChunk, lr);
                        matched = true;
                    }
                }

            if (!matched && node.ReturnUnmatched)
                AppendJoinedRow(outBuilders, resultSchema, sourceSchema, lookupSchema, srcChunk, sr, lookupChunk, -1);
        }

        var columns = outBuilders.Select(b => b.ToColumn()).ToArray();
        return TabularResult.CreateUnvisualized(new InMemoryTableSource(resultSchema, columns));
    }

    private static void AppendJoinedRow(
        BaseColumnBuilder[] builders,
        TableSymbol resultSchema, TableSymbol sourceSchema, TableSymbol lookupSchema,
        ITableChunk srcChunk, int sourceRow, ITableChunk lookupChunk, int lookupRow)
    {
        for (var col = 0; col < resultSchema.Columns.Count; col++)
        {
            var name = resultSchema.Columns[col].Name;
            var srcIdx = IndexOf(sourceSchema, name);
            if (srcIdx >= 0)
            {
                builders[col].Add(srcChunk.Columns[srcIdx].GetRawDataValue(sourceRow));
            }
            else
            {
                var lupIdx = IndexOf(lookupSchema, name);
                builders[col].Add(lookupRow >= 0 && lupIdx >= 0
                    ? lookupChunk.Columns[lupIdx].GetRawDataValue(lookupRow)
                    : null);
            }
        }
    }
}
