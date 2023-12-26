using Kusto.Language.Symbols;

namespace BabyKusto.Core;

public abstract class TypedBaseColumn<T> : BaseColumn

{
    protected TypedBaseColumn(TypeSymbol type)
        : base(type)
    {
    }


    public virtual T? this[int index] => default;
}