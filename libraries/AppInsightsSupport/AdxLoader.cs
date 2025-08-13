using System.Collections.Immutable;
using System.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using Kusto.Data;
using Kusto.Data.Net.Client;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using NotNullStrings;

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
        var kcsb = new KustoConnectionStringBuilder(resourcePath)
            .WithAadUserPromptAuthentication();


        using var kustoClient = KustoClientFactory.CreateCslQueryProvider(kcsb);
        using var response = kustoClient.ExecuteQuery(database, query, null);
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

public static class TableLoaderFromIDataReader
{
    public static IMaterializedTableSource LoadTable(string tableName, IDataReader reader)
    {
        var columns = Enumerable.Range(0, reader.FieldCount)
            .Select(i => new ColumnResult(reader.GetName(i), i,BodgeGetFieldType(i)))
            .ToArray();
        var columnBuilders
            = columns.Select(c => ColumnHelpers.CreateBuilder(c.UnderlyingType, c.Name))
                .ToArray();

        while (reader.Read())
        {
            var row = new object[reader.FieldCount];
            reader.GetValues(row);
            foreach (var bld in columns)
            {
                //special handling of dynamic types which are represented as BinaryData here
                var data = row[bld.Index];
                if (bld.UnderlyingType == typeof(JsonNode) && data is Newtonsoft.Json.Linq.JToken obj) 
                {
                    try
                    {

                        var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
                        data = System.Text.Json.JsonSerializer.Deserialize<JsonNode>(str);
                    }
                    catch
                    {
                        data = null;
                    }
                }

                if (data is DBNull n)
                {
                    data = null;
                }

                columnBuilders[bld.Index].Add(data);
            }
        }

        var rowCount = columnBuilders[0].RowCount;

        var builder = TableBuilder.CreateEmpty("logs", rowCount);
        foreach (var b in columnBuilders)
            builder.WithColumn(b.Name, b.ToColumn());

        var table = builder.ToTableSource();
        return table;

        Type BodgeGetFieldType(int n)
        {
            var t = reader.GetFieldType(n);
            if (t == typeof(object))
                t = typeof(JsonNode);
            return t;
        }
    }
}
