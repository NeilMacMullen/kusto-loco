using System;
using System.Collections;

namespace KustoLoco.Core.DataSource.Columns;

[KustoGeneric(Types = "value")]
public sealed class NullableSet<T> : INullableSet
    where T : class
{
    private readonly BitArray _isNull;
    private readonly T[] _nonnull;
    public readonly bool NoNulls;

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

    public int Length { get; }

    public bool IsNull(int i) => !NoNulls && _isNull[i];
    public object? NullableValue(int i) => IsNull(i) ? null : _nonnull[i];

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
    {   //for parquet which can give use arrays of either nullable or
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
    where T : class
{
    private readonly T?[] _values;
    public readonly bool NoNulls;
    public int Length { get; }

    public static NullableSet_Ref<T> FromObjectsOfCorrectType(object?[] nullableData)
    {
        var length = nullableData.Length;
        var values = new T[length];
        var noNulls = true;
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
        var values = new T[length];
        var noNulls = true;
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

    public NullableSet_Ref(T?[] values, bool noNulls)
    {
        _values = values;
        Length = values.Length;
        NoNulls = noNulls;
    }


#if TYPE_STRING
    public bool IsNull(int i) => false;
    public object? NullableValue(int i) => _values[i] ?? "" ;
#else
    public bool IsNull(int i) => _values[i] == null;
    public object? NullableValue(int i) => _values[i];

#endif
}
