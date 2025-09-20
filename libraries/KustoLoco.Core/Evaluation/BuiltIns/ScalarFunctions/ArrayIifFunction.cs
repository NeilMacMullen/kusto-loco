
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArrayIif")]
internal partial class ArrayIifFunction
{
    private JsonNode? Impl(JsonNode condArray, JsonNode array1, JsonNode array2)
    {
        return ArrayIxfCore.Impl(condArray, array1, array2);
    }
}
