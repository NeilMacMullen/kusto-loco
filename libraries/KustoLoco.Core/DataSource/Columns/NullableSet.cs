using System;
using System.Collections;
using System.Reflection.Metadata.Ecma335;
using KustoLoco.Core.Evaluation.BuiltIns.Impl;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "value")]
public sealed class NullableSet<T> : INullableSet
{
    private readonly BitArray _isNull;
    private readonly T[] _nonnull;

    public NullableSet(T?[] values, bool hasNulls)
    {
        throw new InvalidOperationException("can only be called on ref variant");
    }

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

    public bool NoNulls { get; }
    public Type UnderlyingType { get; } = typeof(T);

    public int Length { get; }

    public bool IsNull(int i) => !NoNulls && _isNull[i];
    public object? NullableObject(int i) => IsNull(i) ? null : _nonnull[i];

    public T? NullableT(int i) => IsNull(i) ? default(T?) : _nonnull[i];

    public Array GetDataAsArray()
    {
        //TODO - we currently can't do this because the main consumer of this API(Parquet serializer)
        //really wants a nullable array
        /*
        if (NoNulls)
            return _nonnull;
        */
        var nullable = new T?[Length];
        for (var i = 0; i < Length; i++)
                nullable[i] = NullableT(i);

        return nullable;
    }

    public static NullableSet<T> FromObjectsOfCorrectType(object?[] nullableData)
    {
        var length = nullableData.Length;
        var isNull = new BitArray(length);
        var nonnull = new T[length];

        for (var i = 0; i < length; i++)
        {
            var d = nullableData[i];
            if (d is not T t)
                isNull[i] = true;
            else
                nonnull[i] = t;
        }

        return new NullableSet<T>(isNull, nonnull);
    }

    public static NullableSet<T> CreateFromBaseArray(Array baseArray)
    {
        //for parquet which can give use arrays of either nullable or
        //non-nullable data
        if (baseArray is T[] nonNullData)
        {
            var length = baseArray.Length;
            return new NullableSet<T>(new BitArray(1), nonNullData);
        }

        if (baseArray is T?[] nullableData)
        {
            var length = baseArray.Length;
            var isNull = new BitArray(length);
            var nonnull = new T[length];

            for (var i = 0; i < length; i++)
            {
                var d = nullableData[i];
                if (d is not T t)
                    isNull[i] = true;
                else
                    nonnull[i] = t;
            }

            return new NullableSet<T>(isNull, nonnull);
        }

        throw new InvalidOperationException();
    }
}

[KustoGeneric(Types = "reference")]
public sealed class NullableSet_Ref<T> : INullableSet
{
    private readonly T?[] _values;
    public bool NoNulls { get; }
    public int Length { get; }
    public Type UnderlyingType { get; } = typeof(T);
    public Array GetDataAsArray()
    {
#if TYPE_STRING
        if (NoNulls)
            return _values;
        var nullable = new T?[Length];
        for (var i = 0; i < Length; i++)
                nullable[i] = NullableT(i);

        return nullable;
#else
        return _values;
#endif
    }


    public static NullableSet_Ref<T> FromObjectsOfCorrectType(object?[] nullableData)
    {
        var length = nullableData.Length;
        var values = new T[length];
        var noNulls = true;
        //TODO- special case for string
        for (var i = 0; i < length; i++)
        {
            var d = nullableData[i];
            if (d is not T t)
                noNulls = false;
            else
                values[i] = t;
        }

        return new NullableSet_Ref<T>(values, noNulls);
    }

    public static NullableSet_Ref<T> CreateFromBaseArray(Array baseArray)
    {
        var nullableData = (T?[])baseArray;
        var length = nullableData.Length;
        var noNulls = true;
        for (var i = 0; i < length; i++)
        {
            var d = nullableData[i];
            if (d is T t) continue;
            noNulls = false;
            break;
        }

        //todo - add special casing for Strings here to use stringpool
        return new NullableSet_Ref<T>(nullableData, noNulls);
    }

    public NullableSet_Ref(T?[] values, bool noNulls)
    {
        _values = values;
        Length = values.Length;
        NoNulls = noNulls;
    }


#if TYPE_STRING
    public bool IsNull(int i) => false;
    public object? NullableObject(int i) => _values[i] ?? string.Empty ;
     public T?  NullableT(int i)=>  _values[i]??string.Empty;
#else
    public bool IsNull(int i) => _values[i] == null;
    public object? NullableObject(int i) => _values[i];
    public T?  NullableT(int i)=>  _values[i];

#endif
}


