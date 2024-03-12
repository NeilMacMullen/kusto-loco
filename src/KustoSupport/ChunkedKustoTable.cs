using KustoLoco.Core;
using Kusto.Language.Symbols;

namespace KustoSupport;

public class ChunkedKustoTable : ITableSource
{
    private readonly int _chunkSize;
    private readonly ITableSource _source;

    private ChunkedKustoTable(ITableSource source, int chunkSize)
    {
        _source = source;
        _chunkSize = chunkSize;
    }


    public TableSymbol Type => _source.Type;

    public IEnumerable<ITableChunk> GetData()
    {
        foreach (var c in _source.GetData())
        {
            foreach (var splitTable in ChunkSplitter.Split(c, _chunkSize))
            {
                yield return splitTable;
            }
        }
    }

    public IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default) =>
        throw new NotImplementedException();

    public static ChunkedKustoTable FromTable(ITableSource source, int chunkSize) => new(source, chunkSize);
}