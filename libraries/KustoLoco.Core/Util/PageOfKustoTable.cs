using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;

namespace KustoLoco.Core.Util;

/// <summary>
/// Represents a page into a table
/// </summary>
/// <remarks>
/// When paginating results it's useful to be able to select a sub-page.
/// The maths is a little tricky because the source data may be split into chunks that
/// have sizes that don't correspond to or align with the page size
/// </remarks>
public class PageOfKustoTable : ITableSource
{
    private readonly int _pageStartOffset;
    private readonly ITableSource _source;
    private readonly int pageSize;

    private PageOfKustoTable(ITableSource source, int pageStartOffset, int pageSize)
    {
        _source = source;
        _pageStartOffset = pageStartOffset;
        this.pageSize = pageSize;
    }

    public TableSymbol Type => _source.Type;

    public IEnumerable<ITableChunk> GetData()
    {
        var chunksReturned = false;
        var chunkOffset = 0;
        foreach (var c in _source.GetData())
        {
            //if this has no data that overlaps with the required subsection then skip it
            var chunkIsBeforeRequiredSection = chunkOffset + c.RowCount <= _pageStartOffset;
            var chunkIsAfterRequiredSection = chunkOffset >= _pageStartOffset + pageSize;
            if (chunkIsBeforeRequiredSection || chunkIsAfterRequiredSection)
            {
                chunkOffset += c.RowCount;
                continue;
            }
            var intersectionStart = Math.Max(chunkOffset, _pageStartOffset) - chunkOffset;
            var intersectionEnd = Math.Min(chunkOffset + c.RowCount, _pageStartOffset + pageSize) - chunkOffset;
            var sliced = ChunkSlicer.Slice(c, intersectionStart, intersectionEnd - intersectionStart);
            chunksReturned = true;
            yield return sliced;
        }
        //ensure that even out of range pages return an empty chunk
        if (!chunksReturned)
            yield return ChunkSlicer.CreateEmptyChunk(_source.GetData().First());
    }

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public static ITableSource Create(ITableSource source, int pageStartOffset, int pageSize)
    {
        return new PageOfKustoTable(source, pageStartOffset, pageSize);
    }
}
