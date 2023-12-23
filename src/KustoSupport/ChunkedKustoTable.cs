using BabyKusto.Core;

namespace KustoSupport;

#pragma warning disable CS8604, CS8602, CS8603
public class ChunkedKustoTable : BaseKustoTable
{
    private readonly int _chunkSize;
    private readonly ITableSource _source;

    private ChunkedKustoTable(BaseKustoTable source, int chunkSize) :base(source.Type,source.Length)
    {
        _source = source;
        _chunkSize = chunkSize;
    }

   
    public override IEnumerable<ITableChunk> GetData()
    {
        foreach (var c in _source.GetData())
        {
            foreach (var splitTable in ChunkSplitter.Split(c, _chunkSize))
            {
                yield return splitTable;
            }
        }
    }

    public override IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default) =>
        throw new NotImplementedException();

    public static ChunkedKustoTable FromTable(BaseKustoTable source, int chunkSize) => new(source, chunkSize);
}