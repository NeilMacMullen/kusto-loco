using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// bag_set_key(bag, key, value) -> a copy of the dynamic object with 'key' set to 'value'; null when 'bag' is not an
// object. The value argument is polymorphic in ADX, so one overload per scalar type.
[KustoImplementation(Keyword = "Functions.BagSetKey")]
internal partial class BagSetKeyFunction
{
    private static JsonNode? StringImpl(JsonNode bag, string key, string value) => DynamicSetSupport.SetKey(bag, key, JsonValue.Create(value));
    private static JsonNode? LongImpl(JsonNode bag, string key, long value) => DynamicSetSupport.SetKey(bag, key, JsonValue.Create(value));
    private static JsonNode? RealImpl(JsonNode bag, string key, double value) => DynamicSetSupport.SetKey(bag, key, JsonValue.Create(value));
    private static JsonNode? BoolImpl(JsonNode bag, string key, bool value) => DynamicSetSupport.SetKey(bag, key, JsonValue.Create(value));
    private static JsonNode? Impl(JsonNode bag, string key, JsonNode value) => DynamicSetSupport.SetKey(bag, key, value);
}
