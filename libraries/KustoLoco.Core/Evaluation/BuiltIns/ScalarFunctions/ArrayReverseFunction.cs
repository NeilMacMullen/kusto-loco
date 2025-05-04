using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArrayReverse")]
internal partial class ArrayReverseFunction
{
    public JsonNode? Impl(JsonNode node)
    {
        if( node is not JsonArray array)
            return null;
       
        var ja = new JsonArray();
        foreach(var n in array.Reverse())
            ja.Add(n?.DeepClone());
        return ja;

    }
}
