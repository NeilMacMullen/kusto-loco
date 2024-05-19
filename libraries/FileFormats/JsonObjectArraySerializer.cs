using System.Collections.Specialized;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using NLog;

namespace KustoLoco.FileFormats;

public class JsonObjectArraySerializer : ITableSerializer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IProgress<string> _progressReporter;
    private readonly KustoSettingsProvider _settings;

    public JsonObjectArraySerializer(KustoSettingsProvider settings, IProgress<string> progressReporter)
    {
        _settings = settings;
        _progressReporter = progressReporter;
        _settings.Register(JsonSerializerSettings.SkipTypeInference);
    }

    public Task<TableSaveResult> SaveTable(string path, KustoQueryResult result)
    {
        var json = result.ToJsonString();
        File.WriteAllText(path, json);
        return Task.FromResult(TableSaveResult.Success());
    }


    public Task<TableLoadResult> LoadTable(string path, string name)
    {
        var text = File.ReadAllText(path);
        var dict = JsonSerializer.Deserialize<OrderedDictionary[]>(text);
        var sdlist = new List<OrderedDictionary>();
        foreach (var d in dict!)
        {
            var s = new OrderedDictionary();
            foreach (var k in d.Keys)
                if (d[k] is JsonElement e && e.ValueKind == JsonValueKind.Array)
                    s[k] = string.Join(";", e.EnumerateArray().Select(i => i.ToString()));
                else
                    s[k] = d[k]?.ToString() ?? string.Empty;
            sdlist.Add(s);
        }

        var table = TableBuilder
            .FromOrderedDictionarySet(name,
                sdlist)
            .ToTableSource();

        if (!_settings.GetBool(JsonSerializerSettings.SkipTypeInference))
            table = TableBuilder.AutoInferColumnTypes(table, _progressReporter);


        return Task.FromResult(TableLoadResult.Success(table));
    }

    private static class JsonSerializerSettings
    {
        //TODO - source generation would allow much more flexibility for
        //self-describing settings  
        private const string prefix = "json";

        public static readonly KustoSettingDefinition SkipTypeInference = new(Setting("skipTypeInference"),
            "skips type inference", "false", nameof(Boolean));

        private static string Setting(string setting)
        {
            return $"{prefix}.{setting}";
        }
    }
}
