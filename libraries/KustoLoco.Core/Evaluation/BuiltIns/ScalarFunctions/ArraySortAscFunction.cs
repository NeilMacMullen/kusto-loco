using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArraySortAsc")]
public partial class ArraySortAscFunction
{
    JsonNode? Impl(JsonNode node)
    {
        return  node is not JsonArray array ? null
            : JsonArrayHelpers.Sort(array,true);
    }
}
