using System;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// Shared helpers for the dynamic bag/set scalars. A separate class because the source generator INLINES an Impl body,
// so an Impl must not call a private same-class helper.
internal static class DynamicSetSupport
{
    // bag_keys(bag) -> the top-level property names of a dynamic object as a string array; null when not an object.
    public static JsonNode? Keys(JsonNode bag)
    {
        if (bag is not JsonObject o) return null;
        var arr = new JsonArray();
        foreach (var kv in o) arr.Add(kv.Key);
        return arr;
    }

    // set_has_element membership by JSON value-equality: compare canonical JSON text so string / number / bool / nested
    // all behave consistently. Null when the set argument is not a dynamic array.
    public static bool? Has(JsonNode set, JsonNode? value)
    {
        if (set is not JsonArray arr) return null;
        var target = value?.ToJsonString();
        foreach (var el in arr)
            if (string.Equals(el?.ToJsonString(), target, StringComparison.Ordinal)) return true;
        return false;
    }
}
