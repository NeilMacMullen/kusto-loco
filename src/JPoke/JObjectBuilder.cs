using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace JPoke;

public enum MatchType
{
    MissingObjectProperty,
    Object,
    IncompleteArray,
    IncompatibleType,
    RanOutOfPath
}

public readonly record struct GetResult(MatchType Match, JObjectBuilder ResultOrParent, JPath Remaining);

public readonly record struct JOptions(bool ShouldCreate);

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

    /// <summary>
    ///     Creates a new builder from valid Json text
    /// </summary>
    public static JObjectBuilder FromJsonText(string text)
    {
        var node = JsonNode.Parse(text);
        return new JObjectBuilder(node);
    }

    public JObjectBuilder Set(string path, object value) => Set(PathParser.Parse(path), value);

    private static JsonArray EnsureContainerHasArrayAt(JsonObject container,
        string arrayName)

    {
        if (container.TryGetPropertyValue(arrayName, out var existingArray))
        {
            return existingArray as JsonArray ?? [];
        }

        var arr = new JsonArray();
        container.Remove(arrayName);
        container.Add(arrayName, arr);
        return arr;
    }

    private static void EnsureContainerHasObjectAt(JsonObject container, string path)
    {
        if (container.TryGetPropertyValue(path, out var existingNode)
            && existingNode is JsonObject)
            return;

        var child = new JsonObject();
        container.Remove(path);
        container.Add(path, child);
    }

    private static void SetValueInArray<T>(JsonArray arr, JPathIndex index, Func<T> fill, T value)
        where T : JsonNode
    {
        var actualIndex = index.EffectiveIndex(arr.Count);
        while (arr.Count <= actualIndex) arr.Add(fill());
        arr[actualIndex] = value;
    }


    public GetResult GetNextForArray(JPath path, JOptions options)
    {
        if (path.Length == 0)
            throw new InvalidOperationException();
        var top = path.Elements.First();
        if (!top.IsIndex)
            throw new InvalidOperationException();

        if (_root is JsonArray arr)
        {
            if (top.Index.EffectiveIndex(arr.Count) >= arr.Count)
                return new GetResult(MatchType.IncompleteArray, this, path);
            var obj = arr.ElementAt(top.Index.EffectiveIndex(arr.Count));
            var builder = new JObjectBuilder(obj);
            return builder.GetNextForObject(path.Descend(), options);
        }

        throw new InvalidOperationException();
    }

    public GetResult Search(string pathstr)
    {
        var path = PathParser.Parse(pathstr);
        var options = new JOptions(false);
        return GetNextForObject(path, options);
    }


    public GetResult GetNextForObject(JPath path, JOptions options)
    {
        if (path.Length == 0)
            return new GetResult(MatchType.Object, this, path);
        var top = path.Elements.First();
        if (_root is JsonObject obj)
        {
            if (obj.ContainsKey(top.Name))
            {
                var container = obj[top.Name];
                if (container != null)
                {
                    var builder = new JObjectBuilder(container);

                    //note - we don't traverse the path for indexers
                    //TODO - if we want to support multi-dimensional arrays
                    //we will need a way of specifying and descending array levels
                    return top.IsIndex
                        ? builder.GetNextForArray(path, options)
                        : builder.GetNextForObject(path.Descend(), options);
                }
            }

            return new GetResult(MatchType.MissingObjectProperty, this, path);
        }

        //otherwise something has gone wrong - we've descended to a terminal
        //element but still have a path to traverse...
        throw new InvalidOperationException();
    }


    /// <summary>
    ///     Returns true if the supplied path is populated
    /// </summary>
    public bool Exists(string pathstr)
    {
        var jpath = PathParser.Parse(pathstr);
        var options = new JOptions(false);

        var (matchType, parent, jPath) = GetNextForObject(jpath, options);
        return matchType == MatchType.Object;
    }

    /// <summary>
    ///     Attempts to remove an object or element
    /// </summary>
    /// <remarks>
    ///     Can't be used for array elements
    /// </remarks>
    public void Remove(string path)
    {
        var jpath = PathParser.Parse(path);
        if (jpath.Top.IsIndex)
            throw new InvalidOperationException("can't remove array element");
        if (path.Length == 0)
            throw new InvalidOperationException("can't remove root node");
        var parentPath = jpath.LeafParent();

        var options = new JOptions(false);
        var (matchType, parent, _) = GetNextForObject(parentPath, options);
        if (matchType == MatchType.Object)
        {
            if (parent.ReferenceNode() is JsonObject obj)
            {
                obj.Remove(jpath.Leaf.Name);
            }
            else
                throw new InvalidOperationException("Attempt to remove item from non-container");
        }
    }

    public JObjectBuilder Set(JPath jpath, object value)
    {
        var options = new JOptions(false);
        var n = 100;
        while (n-- > 0)
        {
            var (matchType, parent, jPath) = GetNextForObject(jpath, options);

            switch (matchType)
            {
                case MatchType.Object:
                    //success
                    return parent;

                case MatchType.MissingObjectProperty:
                    if (jPath.IsTerminal && !jPath.Top.IsIndex)

                        return parent.DirectSet(jPath, value);


                    var parentObject = parent.ReferenceNode() as JsonObject;
                    if (jPath.Top.IsIndex)
                        EnsureContainerHasArrayAt(parentObject, jPath.Top.Name);
                    else
                        EnsureContainerHasObjectAt(parentObject, jPath.Top.Name);

                    break;

                case MatchType.IncompleteArray:
                    if (jPath.IsTerminal)
                    {
                        SetValueInArray(parent.ReferenceNode() as JsonArray, jPath.Top.Index,
                            () => null, ObjectToJsonNode(value));
                        return this;
                    }
                    //add an empty object - TODO - this ignores the case of nested arrays

                    SetValueInArray(parent.ReferenceNode() as JsonArray, jPath.Top.Index,
                        () => null, new JsonObject());
                    break;
                default:
                    throw new NotImplementedException($"unhandled case {matchType}");
            }
        }

        throw new InvalidOperationException("Nesting too deep");
    }


    private JObjectBuilder DirectSet(JPath matchRemaining, object value)
    {
        var objs = _root as JsonObject;
        var vBuilder = ObjectToJsonNode(value);
        objs.Remove(matchRemaining.Top.Name);
        objs.Add(matchRemaining.Top.Name, vBuilder);
        return new JObjectBuilder(vBuilder);
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

    public T GetAsValue<T>()
    {
        if (_root is JsonValue el && el.TryGetValue<T>(out var res))
            return res;
        throw new InvalidOperationException($"Can't read {_root} as {typeof(T).Name}");
    }

    /// <summary>
    ///     Returns a _reference to the current JsonNode held by the builder
    /// </summary>
    /// <remarks>
    ///     Use with care - the contents of the node may change unpredictably
    ///     if the builder continues to be used
    /// </remarks>
    public JsonNode ReferenceNode() => _root;

    /// <summary>
    ///     Returns a clone of the current contents of the builder
    /// </summary>
    /// <remarks>
    ///     It's safe to use this since it can't change
    /// </remarks>
    public JsonNode CloneNode() => JsonNode.Parse(Serialize());

    /// <summary>
    ///     Returns a clone of the current builder
    /// </summary>
    /// <returns></returns>
    public JObjectBuilder Clone() => new(CloneNode());

    public void Copy(string source, string dest)
    {
        var src = Search(source);
        if (src.Match != MatchType.Object)
            return;
        var obj = src.ResultOrParent.CloneNode();
        Set(dest, obj);
    }

    public void Move(string source, string dest)
    {
        Copy(source, dest);
        Remove(source);
    }
}