using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArraySplit")]
internal partial class ArraySplitFunction
{
    public JsonNode? Impl(JsonNode node, long start)
    {
        if (node is not JsonArray array)
            return null;
        var first= JsonArrayHelper.Slice(array, 0, start-1);
        var second = JsonArrayHelper.Slice(array, start,1000000);
        return new JsonArray(first, second);
    }


    public JsonNode? DynamicImpl(JsonNode node, JsonNode indices)
    {
        if (node is not JsonArray array)
            return null;
        if (indices is not JsonArray indicesArray)
            return null;
        var i = indicesArray
            .GetValues<long>()
            .Prepend(0)
            .Append(array.Count)
            .ToArray();
        var lst = new List<JsonNode>();
        for(var p=0; p < i.Length - 1;p++)
            lst.Add(JsonArrayHelper.Slice(array, i[p], i[p+1]-1));
        return new JsonArray(lst.ToArray());

    }
}
