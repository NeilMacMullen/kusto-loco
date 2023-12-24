using BabyKusto.Core;
using Kusto.Language.Symbols;

namespace KustoSupport;

//TODO - temporary abstraction until this is moved into ITableSource
public abstract class BaseKustoTable(TableSymbol type, int length) : ITableSource
{
    public string Name => Type.Name;

    public int Length { get; init; } = length;
    public TableSymbol Type { get; init; } = type;
    public abstract IEnumerable<ITableChunk> GetData();

    public abstract IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default);
}