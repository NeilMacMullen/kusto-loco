using System.Collections.Specialized;
using System.Text.Json;
using KustoSupport;
using NLog;

/// <summary>
///     Reads an array of objects in a json file
/// </summary>
public class JsonArrayTableAdaptor : IFileBasedTableAccess
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var text = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<OrderedDictionary[]>(text);
        var sdlist = new List<OrderedDictionary>();
        foreach (var d in dict!)
        {
            var s = new OrderedDictionary();
            foreach (var k in d.Keys)
            {

                //Logger.Warn($"{k} --> {d[k]!.GetType().Name}");
                if (d[k] is JsonElement e && e.ValueKind == JsonValueKind.Array)
                {
                    s[k] = string.Join(";", e.EnumerateArray().Select(i=>i.ToString()));
                }
                else
                    s[k] = d[k]?.ToString() ?? string.Empty;
            }

            sdlist.Add(s);
        }

        var table = TableBuilder
            .FromOrderedDictionarySet(name,
                sdlist);
        context.AddTable(table);
        return Task.FromResult(true);
    }

    public IReadOnlyCollection<string> SupportedFileExtensions() => [".json"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        var json = result.ToJsonString();
        File.WriteAllText(path, json);
        return Task.CompletedTask;
    }
}