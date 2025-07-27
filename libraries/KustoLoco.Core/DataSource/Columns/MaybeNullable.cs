using System;
using System.Collections;
using System.Text.Json.Nodes;
using KustoLoco.Core.Util;

namespace KustoLoco.Core.DataSource.Columns;

//TODO Note the interface is only used until we remove generic types from Columns
public interface IMaybeNullable
{
    public bool IsNull(int i);
    public object ? NullableValue(int i);
    public int Length { get; }
}

public static class MaybeLocator
{
    public static IMaybeNullable GetNullableForType(Type t, object?[] data)
    {
        var u = TypeMapping.UnderlyingType(t);
        if (u == typeof(bool))
            return new MaybeNullableOfbool(data);
        if (u == typeof(int))
            return new MaybeNullableOfint(data);
        if (u == typeof(long))
            return new MaybeNullableOflong(data);
        if (u == typeof(decimal))
            return new MaybeNullableOfdecimal(data);
        if (u == typeof(double))
            return new MaybeNullableOfdouble(data);
        if (u == typeof(Guid))
            return new MaybeNullableOfGuid(data);
        if (u == typeof(DateTime))
            return new MaybeNullableOfDateTime(data);
        if (u == typeof(TimeSpan))
            return new MaybeNullableOfTimeSpan(data);
        if (u == typeof(string))
            return new MaybeNullableOfstring(data);
        if (u == typeof(JsonNode))
            return new MaybeNullableOfJsonNode(data);
        

        throw new InvalidOperationException($"Unable to create set of type {t.Name}");
    }
}

[KustoGeneric(Types = "value")]
public sealed class MaybeNullable_val<T> :IMaybeNullable
where T:class
{
    
    private readonly BitArray _isNull;
    private readonly T[] _nonnull;
    public readonly bool NoNulls;
    public int Length { get; }
    public MaybeNullable_val(object?[] nullableData)
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

[KustoGeneric(Types = "reference")]
public sealed class MaybeNullable_ref<T> : IMaybeNullable
    where T : class
{
    private readonly T?[] _values;
    public readonly bool NoNulls;
    public int Length { get; }
    public MaybeNullable_ref( object?[] nullableData)
    {
        Length = nullableData.Length;
        _values =  new T[Length] ;
        NoNulls = true;
        for (var i = 0; i < Length; i++)
        {
            var d = nullableData[i];
            if (d is not T t)
                NoNulls = false;
            else
                _values[i] = t;
        }
    }
#if TYPE_STRING
    public bool IsNull(int i) => false;
    public object? NullableValue(int i) => _values[i] ?? "" ;
#else
    public bool IsNull(int i) => _values[i] == null;
    public object? NullableValue(int i) => _values[i];

#endif
}
