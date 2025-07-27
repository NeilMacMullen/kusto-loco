using System.Collections;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "value")]
public sealed class NullableSet<T> :INullableSet
    where T:class
{
    private readonly BitArray _isNull;
    private readonly T[] _nonnull;
    public readonly bool NoNulls;
    public int Length { get; }

    public NullableSet(BitArray isNull, T[] nonnull)
    {
        _isNull = isNull;
        _nonnull = nonnull;
        Length = _nonnull.Length;
        if (!isNull.HasAnySet())
        {
            _isNull = new BitArray(0);
            NoNulls = true;
        } 
    }

    public NullableSet(object?[] nullableData)
    {
        Length = nullableData.Length;
        _isNull = new BitArray(Length);
        _nonnull = new T[Length];

        for (var i = 0; i < Length; i++)
        {
            var d = nullableData[i];
            if (d is not T t)
                _isNull[i] = true;
            else
                _nonnull[i] = t;
        }

        //check if they're all non-null in which case
        //we don't need the bit-array
        if (!_isNull.HasAnySet())
        {
            NoNulls = true;
            _isNull = new BitArray(0);
        }
    }

    public bool IsNull(int i) => !NoNulls && _isNull[i];
    public object ? NullableValue(int i) => IsNull(i) ? null : _nonnull[i];
}
