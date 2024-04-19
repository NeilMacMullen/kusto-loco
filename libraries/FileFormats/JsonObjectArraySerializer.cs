using KustoLoco.Core;
using NLog;
using System.Collections.Specialized;
using System.Text.Json;

namespace KustoLoco.FileFormats;

public class JsonObjectArraySerializer : ITableSerializer
{
    public bool RequiresTypeInference => true;
    public Task<TableSaveResult> SaveTable(string path, KustoQueryResult result, IProgress<string> progressReporter)
    {
        var json = result.ToJsonString();
        File.WriteAllText(path, json);
        return Task.FromResult(TableSaveResult.Success());
    }

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

   
    public Task<TableLoadResult> LoadTable(string path,string name, IProgress<string> progressReporter, KustoSettings settings)
    {
        var text = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<OrderedDictionary[]>(text);
        var sdlist = new List<OrderedDictionary>();
        foreach (var d in dict!)
        {
            var s = new OrderedDictionary();
            foreach (var k in d.Keys)
            {
                if (d[k] is JsonElement e && e.ValueKind == JsonValueKind.Array)
                {
                    s[k] = string.Join(";", e.EnumerateArray().Select(i => i.ToString()));
                }
                else
                    s[k] = d[k]?.ToString() ?? string.Empty;
            }
            sdlist.Add(s);
        }

        var table = TableBuilder
            .FromOrderedDictionarySet(name,
                sdlist)
            .ToTableSource();
        return Task.FromResult(TableLoadResult.Success(table));
    }


}
