using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArraySortDesc")]
public partial class ArraySortDescFunction
{
    JsonNode? Impl(JsonNode node)
    {
        return node is not JsonArray array ? null
            : JsonArrayHelper.Sort(array, false);
    }
}
