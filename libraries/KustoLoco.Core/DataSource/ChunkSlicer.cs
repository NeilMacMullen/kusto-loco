using System;
using System.Linq;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource;

/// <summary>
///     Fetches a subset of a chunk
/// </summary>
/// <remarks>
/// Used to support pagination
/// </remarks>
public static class ChunkSlicer
{
    public static ITableChunk Slice(ITableChunk source, int start, int chunkSize)
    {
        var totalSize = source.Columns[0].RowCount;
        var offset = Math.Min(start, totalSize);
        var top = Math.Min(totalSize, start + chunkSize);
        var takeSize = top - offset;
        if (takeSize == 0)
            return CreateEmptyChunk(source);
        var chunkCols =
            source.Columns.Select(col => ColumnHelpers.MapColumn(col, offset, takeSize)).ToArray();
        return new TableChunk(source.Table, chunkCols);
    }

    public static ITableChunk CreateEmptyChunk(ITableChunk source)
    {
        var zeroCols =
            source.Columns.Select(col => ColumnHelpers.MapColumn(col, 0, 0)).ToArray();
        return new TableChunk(source.Table, zeroCols);
    }
}
