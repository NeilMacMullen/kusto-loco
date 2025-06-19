using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

public abstract class TypedBaseColumn<T> : BaseColumn

{
    protected TypedBaseColumn()
        : base(TypeMapping.SymbolForType(typeof(T)))
    {
    }


    public virtual T? this[int index] => default;
    public override BaseColumn Slide(int padAmount, object? defaultValue)
    {
        var builder = ColumnHelpers.CreateBuilder(typeof(T), "sliced");
        
        for (var i = 0; i < RowCount; i++)
        {
            var sourceIndex = i + padAmount;
            var value = (sourceIndex < 0 || sourceIndex >= RowCount)
                ? defaultValue
                : this[sourceIndex];
           builder.Add(value) ;
        }

        return builder.ToColumn();
    }
}
