//
// Licensed under the MIT License.

using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <summary>
/// Implements the bag_pack (also known as pack) function.
/// Creates a dynamic object (property bag) from a key-value pair.
/// Note: The full Kusto bag_pack supports multiple key-value pairs (bag_pack(key1, value1, key2, value2, ...)),
/// but this implementation currently supports a single key-value pair which is sufficient for common use cases.
/// pack() and pack_dictionary() are deprecated aliases of bag_pack()
/// </summary>
[KustoImplementation(Keyword = "Functions.BagPack")]
internal partial class BagPackFunction
{
    private static JsonNode? Impl(string key, string value)
    {
        var bag = new JsonObject();
        if (!string.IsNullOrEmpty(key))
        {
            bag[key] = JsonValue.Create(value);
        }
        return bag;
    }

    private static JsonNode? JsonImpl(string key, JsonNode? value)
    {
        var bag = new JsonObject();
        if (!string.IsNullOrEmpty(key))
        {
            bag[key] = value?.DeepClone();
        }
        return bag;
    }

    private static JsonNode? IntImpl(string key, int value)
    {
        var bag = new JsonObject();
        if (!string.IsNullOrEmpty(key))
        {
            bag[key] = JsonValue.Create(value);
        }
        return bag;
    }

    private static JsonNode? LongImpl(string key, long value)
    {
        var bag = new JsonObject();
        if (!string.IsNullOrEmpty(key))
        {
            bag[key] = JsonValue.Create(value);
        }
        return bag;
    }

    private static JsonNode? DoubleImpl(string key, double value)
    {
        var bag = new JsonObject();
        if (!string.IsNullOrEmpty(key))
        {
            bag[key] = JsonValue.Create(value);
        }
        return bag;
    }

    private static JsonNode? BoolImpl(string key, bool value)
    {
        var bag = new JsonObject();
        if (!string.IsNullOrEmpty(key))
        {
            bag[key] = JsonValue.Create(value);
        }
        return bag;
    }
}
