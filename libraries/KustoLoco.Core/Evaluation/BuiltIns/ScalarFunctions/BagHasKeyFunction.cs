using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// bag_has_key(bag, key) -> true iff the dynamic object has the given top-level key; null when 'bag' is not an object.
[KustoImplementation(Keyword = "Functions.BagHasKey")]
internal partial class BagHasKeyFunction
{
    private static bool? Impl(JsonNode bag, string key) => DynamicSetSupport.HasKey(bag, key);
}
