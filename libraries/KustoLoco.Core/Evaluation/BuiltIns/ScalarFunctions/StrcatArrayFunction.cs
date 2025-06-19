using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.StrcatArray")]
internal partial class StrcatArrayFunction
{
    public string Impl(JsonNode node, string separator)
    {
        return node is not JsonArray array
            ? string.Empty
            : string.Join(separator, array.Select(a => a?.ToString()));
    }
}
