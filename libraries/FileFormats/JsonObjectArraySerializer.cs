using System.Collections.Specialized;
using System.Text;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using NLog;

namespace KustoLoco.FileFormats;

public class JsonObjectArraySerializer : ITableSerializer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public JsonObjectArraySerializer(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
        _settings.Register(JsonSerializerSettings.SkipTypeInference);
    }

  
    public async  Task<TableSaveResult> SaveTable(string path, KustoQueryResult result)
    {
        await using var stream = File.Create(path);
        return await SaveTable(stream, result);    
    }

    public async Task<TableSaveResult> SaveTable(Stream stream, KustoQueryResult result)
    {
        var json = result.ToJsonString();
        await stream.WriteAsync(Encoding.UTF8.GetBytes(json));
        return TableSaveResult.Success();
    }
    public async Task<TableLoadResult> LoadTable(string path, string tableName)
    {
        await using var stream = File.OpenRead(path);
        return await LoadTable(stream, tableName);  
    }


    public Task<TableLoadResult> LoadTable(Stream stream, string name)
    {

        var dict = JsonSerializer.Deserialize<OrderedDictionary[]>(stream);
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
            table = TableBuilder.AutoInferColumnTypes(table, _console);


        return Task.FromResult(TableLoadResult.Success(table));
    }

    private static class JsonSerializerSettings
    {
        //TODO - source generation would allow much more flexibility for
        //self-describing settings  
        private const string prefix = "json";

        public static readonly KustoSettingDefinition SkipTypeInference = new(
            Setting("skipTypeInference"), "prevents conversion of string columns to types",
            "off",
            nameof(Boolean));

        private static string Setting(string setting)
        {
            return $"{prefix}.{setting}";
        }
    }
}
