using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "array_concat")]
internal partial class ArrayConcatFunction
{
    public JsonNode? Impl(JsonNode node)
    {
        if (node is not JsonArray array)
            return null;
        var result = new JsonArray();
        foreach (var item in array)
        {
            if (item is JsonArray subArray)
            {
                var cloned = JsonArrayHelper.ClonedItems(subArray);
                foreach(var i in cloned)
                    result.Add(i);
            }
            else
            {
                //do nothing ?
            }
        }

        return result;
    }
}
