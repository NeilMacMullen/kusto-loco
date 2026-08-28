using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// set_has_element(set, value) -> true iff the dynamic array 'set' contains 'value' (by JSON value-equality); null when
// 'set' is not a dynamic array. The value argument is polymorphic in ADX, so one overload per scalar type (the source
// generator names each distinctly-suffixed *Impl method as its own overload).
[KustoImplementation(Keyword = "Functions.SetHasElement")]
internal partial class SetHasElementFunction
{
    private static bool? StringImpl(JsonNode set, string value) => DynamicSetSupport.Has(set, JsonValue.Create(value));
    private static bool? LongImpl(JsonNode set, long value) => DynamicSetSupport.Has(set, JsonValue.Create(value));
    private static bool? RealImpl(JsonNode set, double value) => DynamicSetSupport.Has(set, JsonValue.Create(value));
    private static bool? BoolImpl(JsonNode set, bool value) => DynamicSetSupport.Has(set, JsonValue.Create(value));
    private static bool? Impl(JsonNode set, JsonNode value) => DynamicSetSupport.Has(set, value);
}
