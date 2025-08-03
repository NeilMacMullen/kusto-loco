using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArrayRotateLeft")]
internal partial class ArrayRotateLeftFunction
{
    public JsonNode? Impl(JsonNode node, long n) =>
        node is not JsonArray array
            ? null
            : JsonArrayHelper.RotateLeft(array, n);
}
