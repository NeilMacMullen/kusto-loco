using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

public abstract class TypedBaseColumn<T> : BaseColumn

{
    protected TypedBaseColumn()
        : base(TypeMapping.SymbolForType(typeof(T)))
    {
    }


    public virtual T? this[int index] => default;
}