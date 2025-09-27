using System;
using System.Collections;
using CommunityToolkit.HighPerformance.Buffers;

namespace KustoLoco.Core.DataSource.Columns;

public interface INullableSetBuilder
{
    object? this[int index] { get; set; }
    INullableSetBuilder Create(int initialLength, bool canResize);
    void Add(object? value);
    INullableSet ToINullableSet();
}

[KustoGeneric(Types = "value")]
public sealed class NullableSetBuilder<T> : INullableSetBuilder
{
    private readonly bool _canResize;
    private readonly BitArray _isNull;
    private T[] _nonnull;

    public NullableSetBuilder(int initialLength, bool canResize)
    {
        _canResize = canResize;
        _isNull = new BitArray(initialLength);
        _nonnull = new T[initialLength];
    }

    public int Length { get; private set; }

    public object? this[int index]
    {
        get => _isNull[index] ? null : _nonnull[index];
        set
        {
            if (value is not T data)
                _isNull[index] = true;
            else
                _nonnull[index] = data;
        }
    }

    public INullableSet ToINullableSet() => ToNullableSet();

    public void Add(object? value) => Add(value is T d ? d : null);


    public INullableSetBuilder Create(int initialLength, bool canResize) =>
        new NullableSetBuilder<T>(initialLength, canResize);

    public static NullableSetBuilder<T> CreateFixed(int size) => new(size, false);

    //ensure we start with some capacity
    public static NullableSetBuilder<T> CreateExpandable(int size) => new(Math.Max(size, 100), true);

    public void Add(T? value)
    {
        var currentCapacity = _nonnull.Length;

        if (Length >= currentCapacity)
        {
            //need to expand
            var newCapacity = currentCapacity * 2;
            Array.Resize(ref _nonnull, newCapacity);
            _isNull.Length = newCapacity;
        }

        if (value is not T data)
            _isNull[Length] = true;
        else
            _nonnull[Length] = data;
        Length++;
    }

    public NullableSet<T> ToNullableSet()
    {
        if (_canResize)
        {
            Array.Resize(ref _nonnull, Length);
            _isNull.Length = Length;
        }

        return new NullableSet<T>(_isNull, _nonnull);
    }
}
//
// ---------------------------------------------- REFERENCE ---------------------------------------------------
//

[KustoGeneric(Types = "reference")]
public sealed class NullableSetBuilder_ref<T> : INullableSetBuilder
    where T : class
{
    private readonly bool _canResize;
    private T?[] _nonnull;

    public static NullableSetBuilder_ref<T> CreateFixed(int size) => new(size, false);

    public static NullableSetBuilder_ref<T> CreateExpandable(int size) => new(size, true);

    public INullableSetBuilder Create(int initialLength, bool canResize) =>
        new NullableSetBuilder<T>(initialLength, canResize);

    public NullableSetBuilder_ref(int initialLength, bool canResize)
    {
        _canResize = canResize;
        _nonnull = new T[initialLength];
    }


    public int Length { get; private set; }

    private bool _noNulls;
#if TYPE_STRING
    private StringPool pool = new StringPool(1000);
#endif
    public void Add(object? value) => Add(value as T );

    public void Add(T? value)
    {
        var currentCapacity = _nonnull.Length;

        if (Length >= currentCapacity)
        {
            //need to expand
            var newCapacity = currentCapacity * 2;
            Array.Resize(ref _nonnull,newCapacity);
        }

        _nonnull[Length] = value;
        if (value == null) _noNulls = true;
        Length++;
    }

    public NullableSet<T> ToNullableSet()
    {
        if (_canResize)
            Array.Resize(ref _nonnull, Length);
#if TYPE_STRING
        for (var i = 0; i < Length; i++)
        {
            var d = _nonnull[i];
            if (d !=null)
                _nonnull[i] = pool.GetOrAdd(d);
        }
#endif
        return new NullableSet<T>(_nonnull, _noNulls);
    }

    public INullableSet ToINullableSet() => ToNullableSet();

    public object? this[int index]
    {
        get => _nonnull[index];
        set
        {
            if (value is not T data)
            {
                _noNulls = false;
            }
            else
            {
                _nonnull[index] = data;
            }
        }
    }
}
