using System;

namespace KustoLoco.Core.DataSource.Columns;

/// <summary>
/// A column which performs evaluation-time type-conversion
/// </summary>
/// <remarks>
/// When performing type conversion of function parameters, it's useful to be able to
/// create a column that simply applies the conversion as the number is needed.
/// </remarks>
public class OldConvertingColumn<T> : OldTypedBaseColumn<T>
{
  
    public readonly BaseColumn BackingColumn;
    private readonly Func<object?, T> _converter;

    private OldConvertingColumn(BaseColumn backing, Func<object?, T> converter)
    {
        _converter = converter;
        BackingColumn = backing;
    }

    public override T this[int index] => _converter(BackingColumn.GetRawDataValue(index));

    public override int RowCount =>BackingColumn.RowCount;


    public static OldTypedBaseColumn<T> Create(BaseColumn backing,Func<object?,T> converter)
    {
        //in principle some optimisation is possible here if the backing has just a single value... we could
        //convert once ahead of time then create a singlevalue wrapper around the converted value
        //but that's probably a corner case.
        return new OldConvertingColumn<T>(backing,converter);
    }

  

    public override void ForEach(Action<object?> action)
    {
        BackingColumn.ForEach(o=>action(_converter(o)));
    }

    public override BaseColumn Slice(int start, int length)
    {
        return new OldConvertingColumn<T>(BackingColumn.Slice(start, length), _converter);
    }


    public override object? GetRawDataValue(int index) => this[index];
}
