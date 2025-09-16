using Kusto.Data;
using Kusto.Data.Net.Client;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using NotNullStrings;
using System.Collections.Immutable;
using System.Text.Json;

namespace AppInsightsSupport;

public class AdxLoader
{
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public AdxLoader(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
    }

    public async Task<KustoQueryResult> LoadTable(string resourcePath, string database,
        string query)
    {
        var kcsb = new KustoConnectionStringBuilder(resourcePath,database)
            .WithAadUserPromptAuthentication();


        using var kustoClient = KustoClientFactory.CreateCslQueryProvider(kcsb) ;
        using var response = kustoClient.ExecuteQuery(query);
        var table = TableLoaderFromIDataReader.LoadTable("result", response);
        var vs = VisualizationState.Empty;
        if (response.NextResult())
        {
            response.Read();

            var data = response.GetString(0);
            var d =  JsonSerializer.Deserialize<Dictionary<string, object?>>(data)!;
            var ct = d.GetValueOrDefault("Visualization", "");
            if (ct != null)
            {

                var v = ct.ToString().NullToEmpty();
                var id = d
                    .Where(kv=>kv.Value!=null)
                    .ToImmutableDictionary(kv => kv.Key, kv => kv.Value!.ToString().NullToEmpty());
                vs = new VisualizationState(v, id);
            }
        }

        var res = new KustoQueryResult(query, table,
            vs,
            TimeSpan.Zero, string.Empty);
        return await Task.FromResult(res);
    }

    private static VisualizationState StateFromBinaryData(BinaryData viz)
    {
        using var vizDoc = JsonDocument.Parse(viz);
        var queryViz = vizDoc.RootElement.GetProperty("visualization");
        var visState = queryViz.GetString().NullToEmpty();
        var props = vizDoc.RootElement.EnumerateObject().ToImmutableDictionary(
            el => el.Name,
            el => el.Value.ToString().NullToEmpty()
        );
        //TODO - accept other properties
        return new VisualizationState(visState, props);
    }
}
