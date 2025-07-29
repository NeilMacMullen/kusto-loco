using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "all")]
public abstract class GenericTypedBaseColumn<T> : BaseColumn
where T:class
{
    protected GenericTypedBaseColumn()
        : base(TypeMapping.SymbolForType(typeof(T)))
    {
    }


    public virtual T? this[int index] => default;
    public override BaseColumn Slide(int padAmount, object ? defaultValue)
    {
        var builder = ColumnHelpers.CreateBuilder(typeof(T), "sliced");
        
        for (var i = 0; i < RowCount; i++)
        {
            var sourceIndex = i + padAmount;
            var value = (sourceIndex < 0 || sourceIndex >= RowCount)
                ? (T?) (defaultValue)
                : this[sourceIndex];
           builder.Add(value) ;
        }

        return builder.ToColumn();
    }
}
