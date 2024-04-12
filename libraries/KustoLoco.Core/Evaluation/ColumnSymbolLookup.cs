using System.Collections.Generic;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation;

/// <summary>
///     Allows us to look up a column symbol by name and type, rather than by reference.
/// </summary>
/// <remarks>
///     We can't rely on a ColumnSymbol being equatable since it comes from the Kusto
///     Language model and has all sorts of hair
/// </remarks>
public class ColumnSymbolLookup
{
    private readonly Dictionary<IndexedColumnSymbol, int> _indexedColumnSymbols = new();

    public bool TryAdd(ColumnSymbol symbol, int index)
    {
        var lookup = FromColumnSymbol(symbol);
        return _indexedColumnSymbols.TryAdd(lookup, index);
    }

    private IndexedColumnSymbol FromColumnSymbol(ColumnSymbol symbol) => new(symbol.Name, symbol.Type.Name);

    public bool TryGetValue(ColumnSymbol symbol, out int destinationColumnIndex)
    {
        var lookup = FromColumnSymbol(symbol);
        var res = _indexedColumnSymbols.TryGetValue(lookup, out var index);
        destinationColumnIndex = res ? index : 0;
        return res;
    }

    public readonly record struct IndexedColumnSymbol(string Name, string Type);
}
