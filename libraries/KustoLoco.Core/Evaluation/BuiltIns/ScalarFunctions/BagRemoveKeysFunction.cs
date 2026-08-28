using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// bag_remove_keys(bag, keys) -> a copy of the dynamic object with the named top-level keys removed; null when the
// arguments are not an object / array.
[KustoImplementation(Keyword = "Functions.BagRemoveKeys")]
internal partial class BagRemoveKeysFunction
{
    private static JsonNode? Impl(JsonNode bag, JsonNode keys) => DynamicSetSupport.RemoveKeys(bag, keys);
}
