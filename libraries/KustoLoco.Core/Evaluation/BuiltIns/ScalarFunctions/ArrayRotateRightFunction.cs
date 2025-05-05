using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArrayRotateRight")]
internal partial class ArrayRotateRightFunction
{
    public JsonNode? Impl(JsonNode node, long n) =>
        node is not JsonArray array
            ? null
            : JsonArrayHelpers.RotateLeft(array, -n);
}
