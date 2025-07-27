using System;
using System.Collections;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "value")]
public sealed class NullableSetBuilder<T>
    where T : class
{
    private readonly BitArray _isNull;
    private  T[] _nonnull;
    private int _occupied;

    public NullableSetBuilder(int initialLength)
    {
        _isNull = new BitArray(initialLength);
        _nonnull = new T[initialLength];
    }

    private int Length => _nonnull.Length;

    public void Add(T? value)
    {
        var currentLength = Length;

        if (_occupied >= currentLength)
        {
            //need to expand
            Array.Resize(ref _nonnull, currentLength * 2);
            _isNull.Length = currentLength * 2;
        }
        if (value is not T data)
            _isNull[_occupied] = true;
        else
            _nonnull[_occupied] = data;
        _occupied++;
    }

    public INullableSet ToNullableSet()
    {
        Array.Resize(ref _nonnull, _occupied);
        _isNull.Length = _occupied;
        return new NullableSet<T>(_isNull, _nonnull);
    }
}

[KustoGeneric(Types = "reference")]
public sealed class NullableSetBuilder_ref<T>
    where T : class
{
    private T?[] _nonnull;
    private int _occupied;
    

    public NullableSetBuilder_ref(int initialLength)
    {
        _nonnull = new T[initialLength];
    }

    private int Length => _nonnull.Length;
    private bool _noNulls;

    public void Add(T? value)
    {
        var currentLength = Length;

        if (_occupied >= currentLength)
        {
            //need to expand
            Array.Resize(ref _nonnull, currentLength * 2);
        }
        
        _nonnull[_occupied] = value;
        if (value == null) _noNulls = true;
        _occupied++;
    }

    public INullableSet ToNullableSet()
    {
        Array.Resize(ref _nonnull, _occupied);
        return new NullableSet<T>(_nonnull,_noNulls);
    }
}
