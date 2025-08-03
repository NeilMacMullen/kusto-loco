using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal static class JsonArrayHelpers
{
    internal static JsonNode?[] ClonedItems(JsonArray array)
        => array.Select(n => n?.DeepClone()).ToArray();

    internal static JsonArray Reverse(JsonArray array) => new(ClonedItems(array).Reverse().ToArray());

    public static JsonNode RotateLeft(JsonArray array, long shift)
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
    public static JsonArray Sort(JsonArray array,bool ascending)
    {
        var count = array.Count;
        var ordering = Enumerable.Range(0, count).ToList();
        ordering.Sort(new JsonArrayComparer(array, ascending: ascending));

        var result = new JsonNode?[count];
        for (var i = 0; i < count; i++)
        {
            var node = array[ordering[i]];
            // TODO: Find a better way to clone than serialize+deserialize. This is silly.
            result[i] = node == null ? null : JsonNode.Parse(node.ToJsonString());
        }

        return new JsonArray(result);
    }
    private class JsonArrayComparer : IComparer<int>
    {
        private readonly JsonArray _array;
        private readonly bool _ascending;

        public JsonArrayComparer(JsonArray array, bool ascending)
        {
            _array = array ?? throw new ArgumentNullException(nameof(array));
            _ascending = ascending;
        }

        public int Compare(int x, int y)
        {
            var a = _array[x];
            var b = _array[y];

            if (a is JsonValue valueA && b is JsonValue valueB)
            {
                return CompareValues(valueA, valueB);
            }

            if (a is JsonValue)
            {
                return -1;
            }

            if (b is JsonValue)
            {
                return 1;
            }

            return 0;
        }

        private int CompareValues(JsonValue a, JsonValue b)
        {
            var vA = ExtractValue(a);
            var vB = ExtractValue(b);

            var isDoubleA = TryToDouble(vA, out var dA);
            var isDoubleB = TryToDouble(vB, out var dB);

            if (isDoubleA && isDoubleB)
            {
                if (!double.IsNaN(dA) && !double.IsNaN(dB))
                {
                    var v = Math.Sign(dA - dB);
                    return _ascending ? v : -v;
                }

                if (!double.IsNaN(dA))
                {
                    return -1;
                }

                if (!double.IsNaN(dB))
                {
                    return 1;
                }

                return 0;
            }

            if (isDoubleA)
            {
                return -1;
            }

            if (isDoubleB)
            {
                return 1;
            }

            var isStringA = vA is string;
            var isStringB = vB is string;
            if (isStringA && isStringB)
            {
                var v = StringComparer.Ordinal.Compare((string?)vA, (string?)vB);
                return _ascending ? v : -v;
            }

            if (isStringA)
            {
                return -1;
            }

            if (isStringB)
            {
                return 1;
            }

            // TODO: Sort guids, but we don't support them now...
            return 0;


            static object? ExtractValue(JsonValue value)
            {
                var result = value.GetValue<object?>();
                if (result is JsonElement jsonElement)
                {
                    switch (jsonElement.ValueKind)
                    {
                        case JsonValueKind.Number:
                            return jsonElement.GetDouble();
                        case JsonValueKind.String:
                            return jsonElement.GetString();
                        case JsonValueKind.True:
                            return 1;
                        case JsonValueKind.False:
                            return 0;
                        case JsonValueKind.Array:
                        case JsonValueKind.Object:
                        case JsonValueKind.Null:
                        case JsonValueKind.Undefined:
                            return null;
                    }
                }

                return result;
            }

            static bool TryToDouble(object? item, out double value)
            {
                switch (item)
                {
                    case int i:
                        value = i;
                        return true;
                    case long i:
                        value = i;
                        return true;
                    case double i:
                        value = i;
                        return true;
                    case TimeSpan i:
                        value = i.Ticks;
                        return true;
                    case DateTime i:
                        value = i.Ticks;
                        return true;
                }

                value = double.NaN;
                return false;
            }
        }
    }
}


