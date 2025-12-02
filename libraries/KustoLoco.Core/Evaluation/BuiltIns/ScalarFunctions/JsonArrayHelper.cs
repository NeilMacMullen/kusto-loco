using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Kusto.Language.Symbols;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

/// <summary>
///     Provides helper methods for working with <see cref="JsonArray" /> objects.
/// </summary>
internal static class JsonArrayHelper
{
    /// <summary>
    ///     Creates a <see cref="JsonArray" /> from a collection of items.
    ///     If an item is a <see cref="JsonNode" />, it is added directly; otherwise, it is wrapped in a
    ///     <see cref="JsonValue" />.
    /// </summary>
    /// <typeparam name="T">The type of items in the source collection.</typeparam>
    /// <param name="source">The source collection.</param>
    /// <returns>A new <see cref="JsonArray" /> containing the items.</returns>
    public static JsonArray From<T>(ICollection<T> source)
    {
        var array = new JsonNode?[source.Count];
        var i = 0;
        foreach (var item in source)
            // If the item is already a JsonNode, use it directly.
            if (item is JsonNode node)
                array[i++] = node;
            else
                // Otherwise, wrap the item in a JsonValue, or use null if the item is null.
                array[i++] = item == null
                    ? null
                    : JsonValue.Create(item);

        return new JsonArray(array);
    }

    /// <summary>
    ///     Clones each item in the given <see cref="JsonArray" /> using <see cref="JsonNode.DeepClone" />.
    /// </summary>
    /// <param name="array">The array to clone.</param>
    /// <returns>An array of cloned <see cref="JsonNode" /> items.</returns>
    internal static JsonNode?[] ClonedItems(JsonArray array)
        => array.Select(n => n?.DeepClone()).ToArray();


    internal static JsonArray Slice(JsonArray array, long start, long end)
    {
        var raw = new List<JsonNode?>();
        if (start < 0) start = array.Count + start;
        start = Math.Clamp(start, 0, array.Count - 1);
        if (end < 0) end = array.Count + end;
        end = Math.Clamp(end, 0, array.Count - 1);
        for (var i = start; i <= end; i++) raw.Add(array[(int)i]);

        var cloned = raw.Select(r => r?.DeepClone()).ToArray();
        return new JsonArray(cloned);
    }

    /// <summary>
    ///     Returns a new <see cref="JsonArray" /> with the items in reverse order.
    /// </summary>
    /// <param name="array">The array to reverse.</param>
    /// <returns>A reversed <see cref="JsonArray" />.</returns>
    internal static JsonArray Reverse(JsonArray array) => new(ClonedItems(array).Reverse().ToArray());

    /// <summary>
    ///     Rotates the items in the <see cref="JsonArray" /> to the left by the specified shift amount.
    /// </summary>
    /// <param name="array">The array to rotate.</param>
    /// <param name="shift">The number of positions to shift.</param>
    /// <returns>A rotated <see cref="JsonArray" />.</returns>
    public static JsonNode RotateLeft(JsonArray array, long shift)
    {
        var count = array.Count;
        if (count == 0)
            return array;

        // Normalize the shift value to the array length.
        var ashift = shift % count;
        if (ashift < 0) ashift += count;
        if (ashift == 0)
            return array;

        var items = ClonedItems(array);
        var rotated = new JsonNode?[count];
        for (var i = 0; i < count; i++)
        {
            // Calculate the new offset for each item.
            var newOffset = i + ashift;
            rotated[i] = items[newOffset % count];
        }

        return new JsonArray(rotated);
    }

    /// <summary>
    ///     Sorts the items in the <see cref="JsonArray" /> in ascending or descending order.
    /// </summary>
    /// <param name="array">The array to sort.</param>
    /// <param name="ascending">True for ascending order, false for descending.</param>
    /// <returns>A sorted <see cref="JsonArray" />.</returns>
    public static JsonArray Sort(JsonArray array, bool ascending)
    {
        var count = array.Count;
        var ordering = Enumerable.Range(0, count).ToList();
        // Sort indices using a custom comparer.
        ordering.Sort(new JsonArrayComparer(array, ascending));

        var result = new JsonNode?[count];
        for (var i = 0; i < count; i++)
        {
            var node = array[ordering[i]];
            // TODO: Find a better way to clone than serialize+deserialize. This is silly.
            result[i] = node == null ? null : JsonNode.Parse(node.ToJsonString());
        }

        return new JsonArray(result);
    }

    /// <summary>
    ///     Given a JsonNode and a target type, attempts to convert the JsonNode to the appropriate value for that type.
    /// </summary>
    public static object? ConvertJsonNodeToValueOfTargetType(JsonNode? node, TypeSymbol targetType)
    {
        if (node == null)
            return null;

        // Handle different target types
        if (targetType == ScalarTypes.String)
            return node.ToString();

        if (targetType == ScalarTypes.Long)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<long>(out var longValue))
                return longValue;

        if (targetType == ScalarTypes.Int)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<int>(out var intValue))
                return intValue;

        if (targetType == ScalarTypes.Real)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<double>(out var doubleValue))
                return doubleValue;
        if (targetType == ScalarTypes.Decimal)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<decimal>(out var decimalValue))
                return decimalValue;

        if (targetType == ScalarTypes.Bool)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<bool>(out var boolValue))
                return boolValue;
        if (targetType == ScalarTypes.Guid)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<Guid>(out var guidValue))
                return guidValue;

        if (targetType == ScalarTypes.DateTime)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<DateTime>(out var dateTimeValue))
                return dateTimeValue;

        if (targetType == ScalarTypes.TimeSpan)
            if (node is JsonValue jsonValue && jsonValue.TryGetValue<TimeSpan>(out var timespanValue))
                return timespanValue;

        if (targetType == ScalarTypes.Dynamic) return node;

        // Default: return the node as-is
        return node;
    }

    /// <summary>
    ///     Convert a possible JsonArray to an object array of values of the target type.
    /// </summary>
    /// <remarks>
    ///     If the input is NOT a JsonArray, it is treated as a raw value.
    ///     This method has been designed to support the mv-expand operator and might be made more generic in the future.
    /// </remarks>
    public static object?[] ToObjectArrayOfType(object? cellValue, TypeSymbol targetType)
    {
        // Handle dynamic arrays
        if (cellValue is JsonArray jsonArray)
            return jsonArray.Select(element => ConvertJsonNodeToValueOfTargetType(element, targetType)).ToArray();

        // Handle dictionary objects (JsonObject)
        if (cellValue is JsonObject jsonObject)
            return jsonObject
                .Select(kvp => (object?) new JsonObject
                {
                    [kvp.Key] = kvp.Value?.DeepClone()
                        })
                .ToArray();

        // Non-array values (including null) are treated as single-element arrays
        //note that the semantics are a little fuzzy for non-array stuff - not sure whether non-dynamic values
        //should return as null (if, indeed, they can ever be passed)
        if (cellValue is JsonNode node)
            return [ConvertJsonNodeToValueOfTargetType(node, targetType)];
        return [cellValue];
    }


    /// <summary>
    ///     Compares items in a <see cref="JsonArray" /> by their values for sorting.
    /// </summary>
    private class JsonArrayComparer : IComparer<int>
    {
        private readonly JsonArray _array;
        private readonly bool _ascending;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JsonArrayComparer" /> class.
        /// </summary>
        /// <param name="array">The array to compare items from.</param>
        /// <param name="ascending">True for ascending order, false for descending.</param>
        public JsonArrayComparer(JsonArray array, bool ascending)
        {
            _array = array ?? throw new ArgumentNullException(nameof(array));
            _ascending = ascending;
        }

        /// <summary>
        ///     Compares two items by their index in the array.
        /// </summary>
        public int Compare(int x, int y)
        {
            var a = _array[x];
            var b = _array[y];

            // If both are JsonValue, compare their values.
            if (a is JsonValue valueA && b is JsonValue valueB) return CompareValues(valueA, valueB);

            // JsonValue is considered less than other types.
            if (a is JsonValue) return -1;

            if (b is JsonValue) return 1;

            // If neither is JsonValue, treat as equal.
            return 0;
        }

        /// <summary>
        ///     Compares two <see cref="JsonValue" /> objects.
        /// </summary>
        private int CompareValues(JsonValue a, JsonValue b)
        {
            var vA = ExtractValue(a);
            var vB = ExtractValue(b);

            var isDoubleA = TryToDouble(vA, out var dA);
            var isDoubleB = TryToDouble(vB, out var dB);

            // Compare as doubles if possible.
            if (isDoubleA && isDoubleB)
            {
                if (!double.IsNaN(dA) && !double.IsNaN(dB))
                {
                    var v = Math.Sign(dA - dB);
                    return _ascending ? v : -v;
                }

                if (!double.IsNaN(dA)) return -1;

                if (!double.IsNaN(dB)) return 1;

                return 0;
            }

            if (isDoubleA) return -1;

            if (isDoubleB) return 1;

            // Compare as strings if possible.
            var isStringA = vA is string;
            var isStringB = vB is string;
            if (isStringA && isStringB)
            {
                var v = StringComparer.Ordinal.Compare((string?)vA, (string?)vB);
                return _ascending ? v : -v;
            }

            if (isStringA) return -1;

            if (isStringB) return 1;

            // TODO: Sort guids, but we don't support them now...
            return 0;

            // Extracts the underlying value from a <see cref="JsonValue"/>.
            static object? ExtractValue(JsonValue value)
            {
                var result = value.GetValue<object?>();
                if (result is JsonElement jsonElement)
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

                return result;
            }

            // Attempts to convert an object to a double value.
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
