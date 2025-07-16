// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

public static class JsonArrayHelper
{
    public static JsonArray From<T>(ICollection<T> source)
    {
        var array = new JsonNode?[source.Count];
        var i = 0;
        foreach (var item in source)
        {
            if (item is JsonNode node)
                array[i++] =  node;
            else
            {
                array[i++] = item == null
                    ? null
                    : JsonValue.Create(item);
            }
        }

        return new JsonArray(array);
    }
}
