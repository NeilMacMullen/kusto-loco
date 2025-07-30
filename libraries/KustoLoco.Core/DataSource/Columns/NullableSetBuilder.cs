using System;
using System.Collections;

namespace KustoLoco.Core.DataSource.Columns;

public interface INullableSetBuilder
{
     INullableSetBuilder Create(int initialLength, bool canResize);
     void Add(object? value);
     object? this[int index] { get; set; }
     INullableSet ToINullableSet();
}


[KustoGeneric(Types = "value")]
public sealed class NullableSetBuilder<T> :INullableSetBuilder
{
    private readonly bool _canResize;
    private readonly BitArray _isNull;
    private  T[] _nonnull;
    private int _occupied;

    public static NullableSetBuilder<T> CreateFixed(int size)
        => new NullableSetBuilder<T>(size, false);

    //ensure we start with some capacity
    public static NullableSetBuilder<T> CreateExpandable(int size)
        => new NullableSetBuilder<T>(Math.Max(size,100), true);
    
    public NullableSetBuilder(int initialLength, bool canResize)
    {
        _canResize = canResize;
        _isNull = new BitArray(initialLength);
        _nonnull = new T[initialLength];
    }

    public int Length => _nonnull.Length;
    public object? this[int index]
    {
        get => _isNull[index] ? null : _nonnull[index];
        set
        {
            if (value is not T data)
            {
                _isNull[index] = true;
            }
            else
            {
                _nonnull[index] = data;
            }
        }
    }

    public INullableSet ToINullableSet() => ToNullableSet();

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

    public NullableSet<T> ToNullableSet()
    {
        if (_canResize)
        {
            Array.Resize(ref _nonnull, _occupied);
            _isNull.Length = _occupied;
        }

        return new NullableSet<T>(_isNull, _nonnull);
    }

    public void Add(object? value) => Add((value is T d) ?d:null  );
   

    public INullableSetBuilder Create(int initialLength,bool canResize) => new NullableSetBuilder<T>(initialLength,canResize);
}

[KustoGeneric(Types = "reference")]
public sealed class NullableSetBuilder_ref<T> :INullableSetBuilder
    where T : class
{
    private readonly bool _canResize;
    private T?[] _nonnull;
    private int _occupied;

    public static NullableSetBuilder_ref<T> CreateFixed(int size)
        => new NullableSetBuilder_ref<T>(size, false);

    public static NullableSetBuilder_ref<T> CreateExpandable(int size)
        => new NullableSetBuilder_ref<T>(size, true);
    
    public INullableSetBuilder Create(int initialLength,bool canResize) => new NullableSetBuilder<T>(initialLength,canResize);

    public NullableSetBuilder_ref(int initialLength,bool canResize)
    {
        _canResize = canResize;
        _nonnull = new T[initialLength];
    }
    

    public int Length => _nonnull.Length;
    private bool _noNulls;

    public void Add(object? value) => Add(value as T );
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

    public NullableSet<T> ToNullableSet()
    {
        if(_canResize)
            Array.Resize(ref _nonnull, _occupied);
        return new NullableSet<T>(_nonnull,_noNulls);
    }
    public INullableSet ToINullableSet() => ToNullableSet();
    public object? this[int index]
    {
        get =>  _nonnull[index];
        set
        {
            if (value is not T data)
                _noNulls = false;
            else 
                _nonnull[index] = data;
            
        }
    }

}
