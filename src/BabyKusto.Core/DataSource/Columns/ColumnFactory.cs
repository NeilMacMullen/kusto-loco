using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public static class ColumnFactory
{
    public static TypedBaseColumn<T> Create<T>(TypeSymbol type, T[] data) => new InMemoryColumn<T>(type, data);
}