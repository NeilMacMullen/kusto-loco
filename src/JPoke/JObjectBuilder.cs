using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace JPoke;

public class JObjectBuilder
{
    private readonly JsonNode _root;

    private JObjectBuilder(JsonNode root) => _root = root;

    public static JObjectBuilder CreateEmpty() => new(new JsonObject());

    public static JObjectBuilder FromObject(object o)
    {
        var node = ObjectToJsonNode(o);
        return new JObjectBuilder(node);
    }

    public JObjectBuilder Set(string path, object value) => Set(_root, PathParser.Parse(path), value);

    private static T EnsureContainerHasArrayWithSufficientItems<T>(JsonObject container,
        string arrayName, int index,
        Func<T> fill)
        where T : JsonNode
    {
        var arr = new JsonArray();
        if (container.TryGetPropertyValue(arrayName, out var existingArray))
        {
            arr = existingArray as JsonArray;
        }
        else
            container.Add(arrayName, arr);


        while (arr.Count <= index) arr.Add(fill());
        return arr[index] as T;
    }

    public JObjectBuilder Set(JsonNode current, JPath path, object value)
    {
        var container = current as JsonObject;
        var element = path.Elements.First();
        if (!path.IsTerminal)
        {
            JsonNode child = new JsonObject();
            if (element.IsIndex)
            {
                var cnt = EnsureContainerHasArrayWithSufficientItems(
                    container, element.Name, element.Index, () => new JsonObject());

                Set(cnt, path.Descend(), value);
            }
            else
            {
                if (container.TryGetPropertyValue(element.Name, out var existingNode))
                {
                    child = existingNode as JsonObject;
                }
                else
                    container.Add(element.Name, child);
            }

            Set(child, path.Descend(), value);
            return this;
        }

        var newValue = ObjectToJsonNode(value);

        //if is terminal
        if (element.IsIndex)
        {
            var isObject = newValue is JsonObject;
            EnsureContainerHasArrayWithSufficientItems<JsonNode>(container, element.Name, element.Index, () =>
                isObject ? new JsonObject() : null
            );
            var arra = new JsonArray();
            if (element.Index == -1)
                arra.Add(ObjectToJsonNode(value));
            else
                arra.Add(ObjectToJsonNode(value));
            (container[element.Name] as JsonArray)[element.Index] = newValue;
        }
        else
            container.Add(element.Name, newValue);

        return this;
    }

    private static JsonNode ObjectToJsonNode(object obj)
    {
        var txt = JsonSerializer.Serialize(obj);
        var tree = JsonNode.Parse(txt);
        return tree;
    }

    public string Serialize() =>
        _root.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true, TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        });
}