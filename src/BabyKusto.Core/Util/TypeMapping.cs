using System;
using System.Collections.Generic;
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

    public static TypeSymbol SymbolForType(Type type)
    {
        if (NetToKustoLookup.TryGetValue(type, out var ts))
            return ts;
        throw new NotImplementedException($"No TypeSymbol equivalent for .Net type {type.Name}");
    }
}