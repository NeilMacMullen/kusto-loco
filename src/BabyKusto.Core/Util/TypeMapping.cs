using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Util;

public static class TypeMapping
{
    private static readonly Dictionary<Type, KustoType> NetToKustoLookup = new()
    {
        //direct type mappings from net to kusto
        [typeof(int)] = new KustoType(ScalarTypes.Int, true),
        [typeof(long)] = new KustoType(ScalarTypes.Long, true),
        [typeof(double)] = new KustoType(ScalarTypes.Real, true),
        [typeof(string)] = new KustoType(ScalarTypes.String, true),
        [typeof(bool)] = new KustoType(ScalarTypes.Bool, true),
        [typeof(DateTime)] = new KustoType(ScalarTypes.DateTime, true),
        [typeof(decimal)] = new KustoType(ScalarTypes.Decimal, true),
        [typeof(Guid)] = new KustoType(ScalarTypes.Guid, true),
        [typeof(TimeSpan)] = new KustoType(ScalarTypes.TimeSpan, true),
        [typeof(JsonNode)] = new KustoType(ScalarTypes.Dynamic, true),
        //other type mappings we support
        [typeof(short)] = new KustoType(ScalarTypes.Int, false),
        [typeof(ushort)] = new KustoType(ScalarTypes.Int, false),
        [typeof(float)] = new KustoType(ScalarTypes.Real, false),
        //promote to longer type to preserve sign bit
        [typeof(uint)] = new KustoType(ScalarTypes.Long, false),
        [typeof(ulong)] = new KustoType(ScalarTypes.Long, false),
    };

    private static readonly Dictionary<TypeSymbol, Type> KustoToNetToLookup;

    static TypeMapping()
    {
        KustoToNetToLookup = NetToKustoLookup
            .Where(kv => kv.Value.IsPrimary)
            .ToDictionary(kv => kv.Value.Type, kv => kv.Key);
    }

    /// <summary>
    ///     Returns the TypeSymbol associated with the .Net type
    /// </summary>
    /// <remarks>
    ///     Multiple .Net types can map to the same TypeSymbol and this method
    ///     also allows us to map nullable types.  So for example
    ///     float,float?,double,double?  all map to ScalarTypes.Real
    /// </remarks>
    public static TypeSymbol SymbolForType(Type type)
    {
        if (NetToKustoLookup.TryGetValue(UnderlyingType(type), out var ts))
            return ts.Type;
        throw new NotImplementedException($"No TypeSymbol equivalent for .Net type {type.Name}");
    }

    /// <summary>
    ///     Gets the "core" .Net type associated with the TypeSymbol
    /// </summary>
    /// <remarks>
    ///     Even though the data in columns is stored as NULLABLE, we return
    ///     'int' vs 'int?' etc
    /// </remarks>
    public static Type UnderlyingTypeForSymbol(TypeSymbol ts)
    {
        if (KustoToNetToLookup.TryGetValue(ts, out var type))
            return type;
        if (ts is DynamicPrimitiveSymbol ds)
            return UnderlyingTypeForSymbol(ds.UnderlyingType);
        if (ts.Name == "dynamic")
            return UnderlyingTypeForSymbol(ScalarTypes.Dynamic);
        throw new NotImplementedException($"No .Net type  equivalent for typeSymbol {ts.Name}");
    }

    public static bool IsNullable(Type t)
        => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

    public static Type UnderlyingType(Type t) =>
        IsNullable(t)
            ? Nullable.GetUnderlyingType(t)!
            : t;


    /// <summary>
    ///     "Safe" cast to allow short -> nullable-of-int etc
    /// </summary>
    /// <remarks>
    ///     the T is assumed to be nullable
    /// </remarks>
    public static T? CastOrConvertToNullable<T>(object? value) =>
        value == null
            ? default
            : !IsNullable(value.GetType())
                ? (T?)Convert.ChangeType(value, UnderlyingType(typeof(T)))
                : (T?)Convert.ChangeType(value, typeof(T));

    private readonly record struct KustoType(TypeSymbol Type, bool IsPrimary);
}

public static class TypeNameHelper
{
    public static string GetTypeDisplayName<T>(T type)
    {
        return typeof(T).Name;
    }
}