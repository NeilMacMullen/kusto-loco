namespace KustoLoco.Core.DataSource.Columns;

//TODO Note the interface is only used until we remove generic types from Columns

[KustoGeneric(Types = "reference")]
public sealed class NullableSet_Ref<T> : INullableSet
    where T : class
{
    private readonly T?[] _values;
    public readonly bool NoNulls;
    public int Length { get; }
    public NullableSet_Ref( object?[] nullableData)
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
