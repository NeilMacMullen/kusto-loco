using Azure.Core;
using Azure.Identity;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using System.Data;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using static System.Net.WebRequestMethods;

namespace AppInsightsSupport;

public class KqlClient

{
    private readonly KustoSettingsProvider _settings;
    private readonly IKustoConsole _console;

    public KqlClient(KustoSettingsProvider settings, IKustoConsole console)
    {
        _settings = settings;
        _console = console;
    }
    public enum KqlServiceType
    {
        Arg,
        LogAnalytics,
        Defender
    }

    public async Task<KustoQueryResult> LoadKqlAsync(
        KqlServiceType serviceType,
        string param1,
        string query
        )
    {
        string url;
        object payload;
        string sourceType;
        string[] scopes;

        switch (serviceType)
        {
            case KqlServiceType.Arg:
                scopes = new[] { "https://management.azure.com/.default" };
                url = "https://management.azure.com/providers/Microsoft.ResourceGraph/resources?api-version=2022-10-01";
                payload = new
                {
                    subscriptions = new[] { param1 },
                    query = query,
                    options = new { resultFormat = "table" }
                };
                sourceType = "arg";
                break;

            case KqlServiceType.LogAnalytics:
                scopes = new[] { "https://api.loganalytics.io/.default" };
                url = $"https://api.loganalytics.io/v1/workspaces/{param1}/query";
                payload = new { query = query };
                sourceType = "loganalytics";
                break;


            case KqlServiceType.Defender:
                scopes = new[] { "https://api.security.microsoft.com/.default" };
                url = "https://api.security.microsoft.com/api/advancedhunting/run";
                payload = new { Query = query };
                sourceType = "defender";
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, null);
        }

        // Acquire token
        var credential = new DefaultAzureCredential();
        var token = await credential.GetTokenAsync(new TokenRequestContext(scopes));

        // Prepare HttpClient
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);

        // Send request and normalize
       return await SendRequest(query,http, url, payload, sourceType);
       
    }


    private async Task<KustoQueryResult> SendRequest(string query,HttpClient http, string url,object payload,string type)
    {
        var jsonPayload = JsonSerializer.Serialize(payload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
      
        var response = await http.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var raw = await response.Content.ReadAsStringAsync();

            var formatted=raw;
            try
            {
                using var jdoc = JsonDocument.Parse(raw);
                formatted = JsonSerializer.Serialize(
                    jdoc,
                    new JsonSerializerOptions { WriteIndented = true }
                );
            }
            catch 
            {
                // Not valid JSON, just return the raw string
            }
            return KustoQueryResult.FromError(query, formatted);
        }
        // 5. Parse and display results
        await using var stream = await response.Content.ReadAsStreamAsync();
        var dt = await KqlResponseNormalizer.Normalize(stream, type);
        var reader = new DataTableReader(dt);
        var table = TableLoaderFromIDataReader.LoadTable("result", reader);
        return new KustoQueryResult(query, table, VisualizationState.Empty, TimeSpan.Zero, string.Empty);
    }

}

public static class KqlResponseNormalizer
{
    private static Type MapKqlType(string kqlType) =>
     kqlType.ToLowerInvariant() switch
     {
         "string" => typeof(string),
         "guid" => typeof(Guid),
         "datetime" => typeof(DateTime),
         "date" => typeof(DateTime),
         "timespan" => typeof(TimeSpan),
         "bool" => typeof(bool),
         "boolean" => typeof(bool),
         "int" => typeof(int),
         "int32" => typeof(int),
         "long" => typeof(long),
         "int64" => typeof(long),
         "real" => typeof(double),
         "decimal" => typeof(decimal),
         "dynamic" => typeof(object),
         _ => typeof(string) // fallback
     };

    private static object ConvertValue(JsonElement element, Type targetType)
    {
        if (element.ValueKind == JsonValueKind.Null) return DBNull.Value;

        try
        {
            if (targetType == typeof(string)) return element.ToString();
            if (targetType == typeof(Guid)) return element.GetGuid();
            if (targetType == typeof(DateTime)) return element.GetDateTime();
            if (targetType == typeof(TimeSpan)) return TimeSpan.Parse(element.GetString() ?? "0");
            if (targetType == typeof(bool)) return element.GetBoolean();
            if (targetType == typeof(int)) return element.GetInt32();
            if (targetType == typeof(long)) return element.GetInt64();
            if (targetType == typeof(double)) return element.GetDouble();
            if (targetType == typeof(decimal)) return element.GetDecimal();
            return element.ToString();
        }
        catch
        {
            // Fallback if parsing fails
            return element.ToString();
        }
    }

    public static async Task<DataTable> Normalize(Stream stream, string sourceType)
    {
        var doc = await JsonDocument.ParseAsync(stream);
        var table = new DataTable();

        switch (sourceType.ToLowerInvariant())
        {
            case "arg":
                var data = doc.RootElement.GetProperty("data");
                var argCols = data.GetProperty("columns").EnumerateArray().ToList();
                var argTypes = argCols.Select(c => MapKqlType(c.GetProperty("type").GetString()!)).ToList();

                foreach (var (col, type) in argCols.Zip(argTypes))
                    table.Columns.Add(col.GetProperty("name").GetString(), type);

                foreach (var row in data.GetProperty("rows").EnumerateArray())
                    table.Rows.Add(row.EnumerateArray().Select((v, i) => ConvertValue(v, argTypes[i])).ToArray());
                break;

            case "loganalytics":
                var laTable = doc.RootElement.GetProperty("tables")[0];
                var laCols = laTable.GetProperty("columns").EnumerateArray().ToList();
                var laTypes = laCols.Select(c => MapKqlType(c.GetProperty("type").GetString()!)).ToList();

                foreach (var (col, type) in laCols.Zip(laTypes))
                    table.Columns.Add(col.GetProperty("name").GetString(), type);

                foreach (var row in laTable.GetProperty("rows").EnumerateArray())
                    table.Rows.Add(row.EnumerateArray().Select((v, i) => ConvertValue(v, laTypes[i])).ToArray());
                break;

            case "adx":
                var adxPrimary = doc.RootElement.GetProperty("Tables")
                    .EnumerateArray()
                    .First(t => t.GetProperty("TableName").GetString() == "PrimaryResult");

                var adxCols = adxPrimary.GetProperty("Columns").EnumerateArray().ToList();
                var adxTypes = adxCols.Select(c => MapKqlType(c.GetProperty("DataType").GetString()!)).ToList();

                foreach (var (col, type) in adxCols.Zip(adxTypes))
                    table.Columns.Add(col.GetProperty("ColumnName").GetString(), type);

                foreach (var row in adxPrimary.GetProperty("Rows").EnumerateArray())
                    table.Rows.Add(row.EnumerateArray().Select((v, i) => ConvertValue(v, adxTypes[i])).ToArray());
                break;

            case "defender":
                var schema = doc.RootElement.GetProperty("Schema").EnumerateArray().ToList();
                var defTypes = schema.Select(c => MapKqlType(c.GetProperty("Type").GetString()!)).ToList();

                foreach (var (col, type) in schema.Zip(defTypes))
                    table.Columns.Add(col.GetProperty("Name").GetString(), type);

                foreach (var row in doc.RootElement.GetProperty("Results").EnumerateArray())
                {
                    var values = schema.Select((s, i) =>
                    {
                        var name = s.GetProperty("Name").GetString()!;
                        return row.TryGetProperty(name, out var val)
                            ? ConvertValue(val, defTypes[i])
                            : DBNull.Value;
                    }).ToArray();

                    table.Rows.Add(values);
                }
                break;

            default:
                throw new ArgumentException("Unknown sourceType. Use ARG, LogAnalytics, ADX, or Defender.");
        }

        return table;
    }

}

