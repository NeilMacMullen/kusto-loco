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
        return JsonArrayHelper.Slice(array, start, end);

    }
}
