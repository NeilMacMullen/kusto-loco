using KustoLoco.Core.Util;

namespace KustoLoco.Core;

public abstract class TypedBaseColumn<T> : BaseColumn

{
    protected TypedBaseColumn()
        : base(TypeMapping.SymbolForType(typeof(T)))
    {
    }


    public virtual T? this[int index] => default;
}