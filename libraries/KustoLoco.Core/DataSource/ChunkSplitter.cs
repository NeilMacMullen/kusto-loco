using System;
using System.Collections.Generic;
using System.Linq;
using KustoLoco.Core.Util;
using NLog;

namespace KustoLoco.Core.DataSource;

public static class ChunkSplitter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    //TODO - this is just a quick hacky implementation for testing
    //it could be made much more efficient with indirect/mapping columns
    public static IEnumerable<ITableChunk> Split(ITableChunk source, int chunkSize)
    {
        Logger.Info($"splitting chunk of {source.RowCount} into chunks of {chunkSize}");
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

        Logger.Info($"returning {chunks.Count} chunks");
        return chunks;
    }
}