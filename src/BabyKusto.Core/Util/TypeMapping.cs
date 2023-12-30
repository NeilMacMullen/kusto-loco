using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;

namespace BabyKusto.Core.Util;

public static class TypeMapping
{
    private static readonly Dictionary<Type, TypeSymbol> NetToKustoLookup = new()
    {
        [typeof(int?)] = ScalarTypes.Int,
        [typeof(double?)] = ScalarTypes.Real,
        [typeof(string)] = ScalarTypes.String,
        [typeof(bool?)] = ScalarTypes.Bool,
        [typeof(DateTime?)] = ScalarTypes.DateTime,
        [typeof(decimal?)] = ScalarTypes.Decimal,
        [typeof(Guid?)] = ScalarTypes.Guid,
        [typeof(long?)] = ScalarTypes.Long,
        [typeof(TimeSpan?)] = ScalarTypes.TimeSpan,
        [typeof(JsonNode)] = ScalarTypes.Dynamic,
    };


    private static readonly Dictionary<TypeSymbol, Type> KustoToNetToLookup;

    static TypeMapping()
    {
        KustoToNetToLookup = NetToKustoLookup.ToDictionary(kv => kv.Value, kv => kv.Key);
    }

    public static TypeSymbol SymbolForType(Type type)
    {
        if (NetToKustoLookup.TryGetValue(type, out var ts))
            return ts;
        throw new NotImplementedException($"No TypeSymbol equivalent for .Net type {type.Name}");
    }

    public static Type TypeForSymbol(TypeSymbol ts)
    {
        if (KustoToNetToLookup.TryGetValue(ts, out var type))
            return type;
        if (ts is DynamicPrimitiveSymbol ds)
            return TypeForSymbol(ds.UnderlyingType);
        throw new NotImplementedException($"No .Net type  equivalent for typeSymbol {ts.Name}");
    }
}