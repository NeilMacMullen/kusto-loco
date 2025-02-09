using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Kusto.Language.Parsing;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Extensions;
using static KustoLoco.Core.Evaluation.TreeEvaluator;

namespace KustoLoco.Core.Util;

public static class ChunkHelpers
{
    public static ITableChunk Slice(ITableChunk chunk, ImmutableArray<int> rowIds)
    {
        var mappedColumns = chunk.Columns.Select(c => ColumnHelpers.MapColumn(c, rowIds)).ToArray();
        return new TableChunk(chunk.Table, mappedColumns);
    }

    public static ITableChunk Reassemble(ITableChunk[] chunksInThisBucket)
    {
        var columnCount = chunksInThisBucket.First().Columns.Length;
        var mergedColumns = new List<BaseColumn>();
        for (var i = 0; i < columnCount; i++)
        {
            var columnIs = chunksInThisBucket.Select(chk => chk.Columns[i]).ToArray();
            var merged = ColumnHelpers.ReassembleInOrder(columnIs);
            mergedColumns.Add(merged);
        }

        return new TableChunk(chunksInThisBucket.First().Table, mergedColumns.ToArray());
    }
}
