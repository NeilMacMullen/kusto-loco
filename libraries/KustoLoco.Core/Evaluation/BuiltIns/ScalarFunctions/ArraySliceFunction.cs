using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ArraySlice")]
internal partial class ArraySliceFunction
{
    public JsonNode? Impl(JsonNode node, long start, long end)
    {
        if (node is not JsonArray array)
            return null;
        var raw = new List<JsonNode?>();
        if (start < 0) start = array.Count + start;
        start = Math.Clamp(start, 0, array.Count - 1);
        if (end < 0) end = array.Count + end;
        end = Math.Clamp(end, 0, array.Count - 1);
        for (var i = start; i <= end; i++) raw.Add(array[(int)i]);

        var cloned = raw.Select(r => r?.DeepClone()).ToArray();
        return new JsonArray(cloned);
    }
}
