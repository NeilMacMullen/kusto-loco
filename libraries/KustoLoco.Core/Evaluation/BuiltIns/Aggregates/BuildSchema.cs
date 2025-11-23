//
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Nodes;
using Kusto.Language.Symbols;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

internal class BuildSchemaFunctionImpl : IAggregateImpl
{
    public EvaluationResult Invoke(ITableChunk chunk, ColumnarResult[] arguments)
    {
        MyDebug.Assert(arguments.Length == 1);
        var valuesColumn = (GenericTypedBaseColumnOfJsonNode)arguments[0].Column;

        var schema = BuildSchemaFromValues(valuesColumn);

        return new ScalarResult(ScalarTypes.Dynamic, schema);
    }

    private static JsonNode BuildSchemaFromValues(GenericTypedBaseColumnOfJsonNode column)
    {
        var mergedSchema = new Dictionary<string, object>();

        for (var i = 0; i < column.RowCount; i++)
        {
            var value = column[i];
            if (value == null) continue;

            if (value is JsonObject obj)
            {
                MergeObjectIntoSchema(obj, mergedSchema);
            }
        }

        return ConvertSchemaToJsonNode(mergedSchema);
    }

    private static void MergeObjectIntoSchema(JsonObject obj, Dictionary<string, object> schema)
    {
        foreach (var kvp in obj)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (!schema.ContainsKey(key))
            {
                schema[key] = InferType(value);
            }
            else
            {
                schema[key] = MergeTypes(schema[key], InferType(value));
            }
        }
    }

    private static object InferType(JsonNode? value)
    {
        if (value == null)
            return "object"; // null values are treated as generic object type in schema

        return value switch
        {
            JsonValue jsonValue => InferJsonValueType(jsonValue),
            JsonObject obj => InferObjectType(obj),
            JsonArray arr => InferArrayType(arr),
            _ => "object"
        };
    }

    private static object InferJsonValueType(JsonValue value)
    {
        if (value.TryGetValue<bool>(out _))
            return "bool";
        if (value.TryGetValue<long>(out _))
            return "long";
        if (value.TryGetValue<int>(out _))
            return "long";
        if (value.TryGetValue<double>(out _))
            return "double";
        if (value.TryGetValue<string>(out _))
            return "string";
        if (value.TryGetValue<DateTime>(out _))
            return "datetime";

        return "object";
    }

    private static object InferObjectType(JsonObject obj)
    {
        var nestedSchema = new Dictionary<string, object>();
        foreach (var kvp in obj)
        {
            nestedSchema[kvp.Key] = InferType(kvp.Value);
        }
        return nestedSchema;
    }

    private static object InferArrayType(JsonArray arr)
    {
        if (arr.Count == 0)
        {
            return new Dictionary<string, object> { { "indexer", "object" } };
        }

        object? elementType = null;
        foreach (var item in arr)
        {
            var itemType = InferType(item);
            elementType = elementType == null ? itemType : MergeTypes(elementType, itemType);
        }

        return new Dictionary<string, object> { { "indexer", elementType ?? "object" } };
    }

    private static object MergeTypes(object type1, object type2)
    {
        // If types are identical, return one of them
        if (type1.Equals(type2))
            return type1;

        // If one is a string type and another is also a string type, they're the same
        if (type1 is string str1 && type2 is string str2)
        {
            if (str1 == str2)
                return str1;

            // Create a list of possible types
            var typeList = new List<string>();
            if (!typeList.Contains(str1))
                typeList.Add(str1);
            if (!typeList.Contains(str2))
                typeList.Add(str2);
            return typeList;
        }

        // If one is a list and the other is a string, merge them
        if (type1 is List<string> list1 && type2 is string str3)
        {
            if (!list1.Contains(str3))
                list1.Add(str3);
            return list1;
        }

        if (type1 is string str4 && type2 is List<string> list2)
        {
            if (!list2.Contains(str4))
                list2.Add(str4);
            return list2;
        }

        // If both are lists, merge them
        if (type1 is List<string> list3 && type2 is List<string> list4)
        {
            foreach (var item in list4)
            {
                if (!list3.Contains(item))
                    list3.Add(item);
            }
            return list3;
        }

        // If both are dictionaries (objects or arrays), merge them
        if (type1 is Dictionary<string, object> dict1 && type2 is Dictionary<string, object> dict2)
        {
            // Check if they're both array types (have "indexer" key)
            if (dict1.ContainsKey("indexer") && dict2.ContainsKey("indexer"))
            {
                return new Dictionary<string, object>
                {
                    { "indexer", MergeTypes(dict1["indexer"], dict2["indexer"]) }
                };
            }

            // Otherwise, merge as objects
            var merged = new Dictionary<string, object>(dict1);
            foreach (var kvp in dict2)
            {
                if (merged.ContainsKey(kvp.Key))
                {
                    merged[kvp.Key] = MergeTypes(merged[kvp.Key], kvp.Value);
                }
                else
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }
            return merged;
        }

        // Default: create a list of both types
        var result = new List<string>();
        if (type1 is string s1)
            result.Add(s1);
        if (type2 is string s2)
            result.Add(s2);
        return result.Count > 0 ? (object)result : "object";
    }

    private static JsonNode ConvertSchemaToJsonNode(Dictionary<string, object> schema)
    {
        var result = new JsonObject();

        foreach (var kvp in schema)
        {
            result[kvp.Key] = ConvertTypeToJsonNode(kvp.Value);
        }

        return result;
    }

    private static JsonNode? ConvertTypeToJsonNode(object typeObj)
    {
        return typeObj switch
        {
            string str => JsonValue.Create(str),
            List<string> list => new JsonArray(list.Select(s => JsonValue.Create(s)).ToArray<JsonNode>()),
            Dictionary<string, object> dict => ConvertSchemaToJsonNode(dict),
            _ => JsonValue.Create("object")
        };
    }
}
