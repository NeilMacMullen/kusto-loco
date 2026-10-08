using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using KustoLoco.Core;
using KustoLoco.Core.DataSource;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace LokqlMcp;

[McpServerToolType]
[Description("This tool allows you to query information about your data")]
public sealed class KqlTools(TableStore store, RequestLog log)
{
    [McpServerTool(Name = "ListTables")]
    [Description("Lists all tables available for querying, with their row and column counts.")]
    public string ListTables()
    {
        var sw = Stopwatch.StartNew();
        var tables = store.Tables().OrderBy(t => t.Name).ToArray();
        log.Write("ListTables", null, $"Returned {tables.Length} tables", sw.Elapsed);
        if (tables.Length == 0)
            return "No tables are loaded.";
        var sb = new StringBuilder();
        sb.AppendLine("name,rows,columns");
        foreach (var t in tables)
            sb.AppendLine($"{t.Name},{RowCount(t)},{t.Type.Columns.Count}");
        return sb.ToString();
    }

    [McpServerTool(Name = "DescribeTable")]
    [Description("Describes a table: its schema (column names and KQL types) followed by a few sample rows as CSV.")]
    public async Task<CallToolResult> DescribeTable(
        [Description("Name of the table")] string tableName,
        [Description("Number of sample rows to include")] int sampleRows = 5)
    {
        var sw = Stopwatch.StartNew();
        var table = store.FindTable(tableName);
        if (table is null)
        {
            log.Write("DescribeTable", tableName, "Table not found", sw.Elapsed);
            return Error($"Table '{tableName}' not found. Available tables: {string.Join(", ", store.Tables().Select(t => t.Name))}");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Table: {table.Name}");
        sb.AppendLine($"Rows: {RowCount(table)}");
        sb.AppendLine("Schema:");
        foreach (var c in table.Type.Columns)
            sb.AppendLine($"  {c.Name}: {c.Type.Name}");

        var result = await store.Context.RunQuery($"['{table.Name}'] | take {Math.Max(0, sampleRows)}");
        if (string.IsNullOrWhiteSpace(result.Error))
        {
            sb.AppendLine();
            sb.AppendLine("Sample rows (CSV):");
            sb.Append(CsvFormatter.Format(result, sampleRows));
        }

        log.Write("DescribeTable", tableName,
            $"Returned schema ({table.Type.Columns.Count} columns) and up to {Math.Max(0, sampleRows)} sample rows",
            sw.Elapsed);
        return Text(sb.ToString());
    }

    [McpServerTool(Name = "RunQuery")]
    [Description(
        "Runs a KQL query against the loaded tables. Returns an error message if the query fails, a PNG image if the query ends with a 'render' chart operator, or otherwise the result as CSV. Call GetKqlHelp to learn about differences from Azure Data Explorer.")]
    public async Task<CallToolResult> RunQuery(
        [Description("The KQL query to run")] string query,
        [Description("Maximum number of rows to return for tabular results")] int maxRows = 100)
    {
        var sw = Stopwatch.StartNew();
        KustoQueryResult result;
        try
        {
            result = await store.Context.RunQuery(query);
        }
        catch (Exception ex)
        {
            log.Write("RunQuery", query, "Returned error to client (exception)", sw.Elapsed, ex.Message);
            return Error(ex.Message);
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            log.Write("RunQuery", query, "Returned error to client", sw.Elapsed, result.Error);
            return Error(result.Error);
        }

        var png = ChartRenderer.TryRender(result, store.Settings, out var renderError);
        if (png is not null)
        {
            log.Write("RunQuery", query,
                $"Rendered PNG chart ({result.Visualization.ChartType}, {result.RowCount} rows, {png.Length / 1024} KB)",
                sw.Elapsed);
            return new CallToolResult
            {
                Content =
                [
                    new TextContentBlock
                    {
                        Text = $"Rendered {result.Visualization.ChartType} chart from {result.RowCount} rows."
                    },
                    ImageContentBlock.FromBytes(png, "image/png")
                ]
            };
        }

        maxRows = Math.Max(0, maxRows);
        var sb = new StringBuilder();
        if (renderError is not null)
            sb.AppendLine($"Note: chart rendering failed ({renderError}); returning tabular data.");
        else if (result.IsChart && ChartRenderer.IsMap(result))
            sb.AppendLine("Note: map visualizations cannot be rendered; returning tabular data.");
        sb.AppendLine(result.RowCount > maxRows
            ? $"Rows: {result.RowCount} (truncated to first {maxRows})"
            : $"Rows: {result.RowCount}");
        sb.Append(CsvFormatter.Format(result, maxRows));
        var returned = Math.Min(result.RowCount, maxRows);
        var action = renderError is not null
            ? $"Chart rendering failed, returned CSV ({returned} of {result.RowCount} rows)"
            : result.IsChart && ChartRenderer.IsMap(result)
                ? $"Map visualization not renderable, returned CSV ({returned} of {result.RowCount} rows)"
                : $"Returned CSV ({returned} of {result.RowCount} rows{(result.RowCount > maxRows ? ", truncated" : "")})";
        log.Write("RunQuery", query, action, sw.Elapsed, renderError);
        return Text(sb.ToString());
    }

    [McpServerTool(Name = "GetKqlHelp")]
    [Description(
        "Returns help on the KQL dialect supported by this engine (KustoLoco), including unsupported operators, differences from Azure Data Explorer, and extra functions. Optionally filter by a topic keyword.")]
    public string GetKqlHelp(
        [Description("Optional topic keyword (e.g. 'render', 'join', 'functions')")] string? topic = null)
    {
        var sw = Stopwatch.StartNew();
        var help = LoadHelp();
        log.Write("GetKqlHelp", string.IsNullOrWhiteSpace(topic) ? "(none)" : topic, "Returned help", sw.Elapsed);
        if (string.IsNullOrWhiteSpace(topic))
            return help;

        var sections = help.Split("\n## ")
            .Where(s => s.Contains(topic, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return sections.Length == 0
            ? $"No help found for topic '{topic}'.\n\n{help}"
            : string.Join("\n## ", sections);
    }

    private static string LoadHelp()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LokqlMcp.KqlHelp.md");
        if (stream is null)
            return "Help is not available.";
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string RowCount(ITableSource t) =>
        t is IMaterializedTableSource m ? m.RowCount.ToString() : "unknown";

    private static CallToolResult Text(string text) =>
        new() { Content = [new TextContentBlock { Text = text }] };

    private static CallToolResult Error(string text) =>
        new() { IsError = true, Content = [new TextContentBlock { Text = text }] };
}
