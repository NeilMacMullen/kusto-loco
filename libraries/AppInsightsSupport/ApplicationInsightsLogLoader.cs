using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using NotNullStrings;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Nodes;


namespace AppInsightsSupport;

public class ApplicationInsightsLogLoader
{
    private readonly IKustoConsole _console;
    private readonly KustoSettingsProvider _settings;

    public ApplicationInsightsLogLoader(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
    }

    public async Task<KustoQueryResult> LoadTable(string tenantId,string resourcePath,
        string query,
        DateTime start, DateTime end
    )
    {
        var credentialOptions = tenantId.IsBlank()
            ? new DefaultAzureCredentialOptions()
            : new DefaultAzureCredentialOptions()
            {
                TenantId = tenantId
            };

        var client = new LogsQueryClient(new DefaultAzureCredential(credentialOptions));

        //try to lookup resource path from settings
        resourcePath = _settings.TrySubstitute(resourcePath);

        var results = await client.QueryResourceAsync(
            new ResourceIdentifier(resourcePath),
            query,
            new QueryTimeRange(start, end),
            new LogsQueryOptions
            {
                IncludeVisualization = true
            });


        var error = results.Value.Error?.Message ?? string.Empty;

        if (error.IsNotBlank())
            return KustoQueryResult.FromError(query, error);


        var resultTable = results.Value.Table;

        var builder = TableBuilder.CreateEmpty("logs", resultTable.Rows.Count);

        foreach (var column in resultTable.Columns)
        {
            _console.Progress("Name: " + column.Name + " Type: " + column.Type);
            var objects = resultTable.Rows.Select(row => row[column.Name])
                .Cast<object?>()
                .ToArray();
            var type = TypeMapping.TypeFromName(column.Type.ToString());
            
            if (type == typeof(JsonNode))
            {
                objects = objects.Select(JsonNodeFromBinaryData).ToArray();
            }
            
            if (type == typeof(DateTime))
                //grr - got to love the inconsistency - 'dateTime' is used for columns of DateTimeOffset
                objects = objects.Select(o => o is DateTimeOffset offset ? (DateTime?)offset.DateTime : null)
                    .Cast<object?>()
                    .ToArray();
           
            var columnBuilder = ColumnHelpers.CreateFromObjectArray(objects, TypeMapping.SymbolForType(type));

            builder.WithColumn(column.Name, columnBuilder);
        }

        var viz = results.Value.GetVisualization();
        var vizState = StateFromBinaryData(viz);

        var table = builder.ToTableSource() as InMemoryTableSource;
        return new KustoQueryResult(query, table!,
            vizState,
            TimeSpan.Zero, string.Empty);
    }

    private object? JsonNodeFromBinaryData(object? data)
    {
        return data switch
        {
            JsonNode node => node,
            BinaryData bd =>  JsonSerializer.Deserialize<JsonNode>(bd),
            _ => null
        };
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
        return new VisualizationState(visState, props);
    }
}
