using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

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

    public static IMaterializedTableSource FromITableSource(ITableSource other)
    {
        /*
         //skip for now until we have  way of just measuring number of chunks
        //if only one chunk, nothing to reassemble    
        var chunks = other.GetData().ToArray();
        if (chunks.Length <= 1)
            return other;
        */
        return InMemoryTableSource.FromITableSource(other);
    }
}
