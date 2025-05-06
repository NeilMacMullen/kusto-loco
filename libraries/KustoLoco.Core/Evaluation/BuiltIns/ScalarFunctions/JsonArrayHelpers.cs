using System.Linq;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class JsonArrayHelpers
{
    internal static JsonNode?[] ClonedItems(JsonArray array)
        => array.Select(n => n?.DeepClone()).ToArray();

    internal static JsonArray Reverse(JsonArray array) => new(ClonedItems(array).Reverse().ToArray());

    public static JsonNode? RotateLeft(JsonArray array, long shift)
    {
        var count = array.Count;
        if (count == 0)
            return array;

        var ashift = shift % count;
        if (ashift < 0) ashift += count;
        if (ashift == 0)
            return array;

        var items = ClonedItems(array);
        var rotated = new JsonNode?[count];
        for (var i = 0; i < count; i++)
        {
            var newOffset = i + ashift;
            rotated[i] = items[newOffset % count];
        }

        return new JsonArray(rotated);
    }
}
