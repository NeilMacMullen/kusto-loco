using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

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


    public JObjectBuilder Set(JsonNode current, PathParser.JPath path, object value)
    {
        var container = current as JsonObject;
        var element = path.Elements.First();
        if (!path.IsTerminal)
        {
            JsonNode child = new JsonObject();
            if (element.IsIndex)
            {
                JsonArray arr = new JsonArray();
                if (container.TryGetPropertyValue(element.Name, out var existingArray))
                {
                    arr = existingArray as JsonArray;
                }
                else
                    container.Add(element.Name, arr);

                var i = element.Index;
                while(arr.Count <= i) arr.Add(new JsonObject());
                var cnt = arr[element.Index] as JsonObject;

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

        if (element.IsIndex)
        {
            var arra = new JsonArray();
            if (element.Index == -1)
                arra.Add(ObjectToJsonNode(value));
            else
                arra.Add(ObjectToJsonNode(value));
            container.Add(element.Name, arra);
        }
        else
            container.Add(element.Name, ObjectToJsonNode(value));

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