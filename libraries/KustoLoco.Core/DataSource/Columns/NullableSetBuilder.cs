using System.Collections;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "value")]
public sealed class NullableSetBuilder<T>
    where T : class
{
    private readonly BitArray _isNull;
    private readonly T[] _nonnull;
    private int _index;

    public NullableSetBuilder(int length)
    {
        Length = length;
        _isNull = new BitArray(Length);
        _nonnull = new T[Length];
    }

    private int Length { get; }

    public void Add(T? value)
    {
        if (value is not T data)
            _isNull[_index] = true;
        else
            _nonnull[_index] = data;
        _index++;
    }

    public INullableSet ToNullableSet() => new NullableSet<T>(_isNull, _nonnull);
}
