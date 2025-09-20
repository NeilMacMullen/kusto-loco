using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArraySum")]
internal partial class ArraySumFunction
{
    public double? Impl(JsonNode node)
    {
        if (node is not JsonArray array)
            return null;

        return array.GetValues<double>()
            .Sum();
    }
}
