using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArrayReverse")]
internal partial class ArrayReverseFunction
{
    public JsonNode? Impl(JsonNode node)
        => node is not JsonArray array
            ? null
            : JsonArrayHelpers.Reverse(array);
}
