using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation;

public class NullTypeSymbol : TypeSymbol
{
    public static readonly TypeSymbol Instance = new NullTypeSymbol();

    private NullTypeSymbol() : base(string.Empty)
    {
    }
}