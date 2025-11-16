using System;
using System.Collections.Generic;
using System.Linq;
using KustoLoco.Core.Util;


namespace KustoLoco.Core.DataSource;

/// <summary>
/// Splits a single large chunk into multiple smaller chunks
/// </summary>
/// <remarks>
/// Primarily used for testing chunk spanning operations.  There's not really anywhere
/// in the real codebase where we deliberately split a chunk into smaller chunks
/// </remarks>
public static class ChunkSplitter
{
    

    //TODO - this is just a quick hacky implementation for testing
    //it could be made much more efficient with indirect/mapping columns
    public static IEnumerable<ITableChunk> Split(ITableChunk source, int chunkSize)
    {
        var totalSize = source.Columns[0].RowCount;
        var offset = 0;

        var chunks = new List<ITableChunk>();
        while (offset < totalSize)
        {
            var takeSize = Math.Min(chunkSize, totalSize - offset);
            var chunkCols =
                source.Columns.Select(col => ColumnHelpers.MapColumn(col, offset, takeSize)).ToArray();
            chunks.Add(new TableChunk(source.Table, chunkCols));
            offset += takeSize;
        }

        return chunks;
    }
}
