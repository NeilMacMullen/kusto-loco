using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// bag_zip(keys, values) -> a dynamic object pairing keys[i] with values[i]. Null when either argument is not a
// dynamic array.
[KustoImplementation(Keyword = "Functions.BagZip")]
internal partial class BagZipFunction
{
    private static JsonNode? Impl(JsonNode keys, JsonNode values) => DynamicSetSupport.Zip(keys, values);
}
