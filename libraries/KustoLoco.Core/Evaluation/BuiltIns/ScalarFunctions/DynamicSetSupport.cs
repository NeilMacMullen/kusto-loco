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

    // bag_has_key(bag, key) -> true iff the top-level key is present; null when the bag argument is not an object.
    public static bool? HasKey(JsonNode bag, string key)
    {
        if (bag is not JsonObject o) return null;
        return o.ContainsKey(key);
    }

    // bag_remove_keys(bag, keys) -> a copy of the bag without the named top-level keys; null when the arguments are not
    // an object / array.
    public static JsonNode? RemoveKeys(JsonNode bag, JsonNode keys)
    {
        if (bag is not JsonObject o || keys is not JsonArray arr) return null;
        var clone = (JsonObject)o.DeepClone();
        foreach (var k in arr)
        {
            var key = k?.ToString();
            if (key != null) clone.Remove(key);
        }
        return clone;
    }

    // bag_set_key(bag, key, value) -> a copy of the bag with key set to value; null when the bag argument is not an
    // object. A null bag key argument is a no-op copy.
    public static JsonNode? SetKey(JsonNode bag, string? key, JsonNode? value)
    {
        if (bag is not JsonObject o) return null;
        var clone = (JsonObject)o.DeepClone();
        if (key != null) clone[key] = value?.DeepClone();
        return clone;
    }

    // bag_zip(keys, values) -> an object pairing keys[i] with values[i]; a longer keys array pads with null, a longer
    // values array ignores the surplus. Null when either argument is not a dynamic array.
    public static JsonNode? Zip(JsonNode keys, JsonNode values)
    {
        if (keys is not JsonArray ka || values is not JsonArray va) return null;
        var obj = new JsonObject();
        for (var i = 0; i < ka.Count; i++)
        {
            var key = ka[i]?.ToString();
            if (key == null) continue;
            obj[key] = i < va.Count ? va[i]?.DeepClone() : null;
        }
        return obj;
    }
}
