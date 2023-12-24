using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BabyKusto.Core.Util;

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