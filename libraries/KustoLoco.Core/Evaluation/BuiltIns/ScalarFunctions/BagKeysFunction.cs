using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// bag_keys(bag) -> the top-level property names of a dynamic property bag as a string array; null when the argument is
// not a dynamic object.
[KustoImplementation(Keyword = "Functions.BagKeys")]
internal partial class BagKeysFunction
{
    private static JsonNode? Impl(JsonNode bag) => DynamicSetSupport.Keys(bag);
}
