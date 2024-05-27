using System.Net.Http.Json;
using KustoLoco.Core;
using KustoLoco.FileFormats;
using KustoLoco.Rendering;
using NotNullStrings;
using Spectre.Console;

var client = new HttpClient();


Console.WriteLine("Usage: webappclient.exe serverURL query");
Console.WriteLine(
    "Example: .\\WebAppClient.exe https://localhost:5000 \"data | summarize count() by Summary | render piechart\"");
if (args.Length != 2)
{
    Console.WriteLine("Please provide the server URL and the query to run");
    return;
}

var query = args[1];
var url = $"{args[0]}/Kql?query={Uri.EscapeDataString(query)}";
Console.WriteLine($"Getting:  '{url}'");
try
{
    var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();
    var responseBody = await response.Content.ReadFromJsonAsync<KustoResultDto>();
    var result = await ParquetResultSerializer.Default.Deserialize(responseBody!);
    RenderResult(result);
}
catch (Exception e)
{
    Console.WriteLine("\nException:");
    Console.WriteLine("Message :{0} ", e.Message);
}


static void RenderResult(KustoQueryResult queryResult)
{
    //display the results as a pretty Spectre.Console table
    var table = new Table();

    // Add columns with header names
    foreach (var column in queryResult.ColumnDefinitions()) table.AddColumn(column.Name);

    // Add rows.  Note that cells could contain nulls in the general case
    foreach (var row in queryResult.EnumerateRows())
    {
        var rowCells = row.Select(CellToString).ToArray();
        table.AddRow(rowCells);
    }

    AnsiConsole.Write(table);

    //render the results as a chart if we were asked to do that
    KustoResultRenderer.RenderChartInBrowser(queryResult);
    if (queryResult.Error.IsNotBlank()) AnsiConsole.MarkupLineInterpolated($"[red]{queryResult.Error}[/]");

    return;

    string CellToString(object? cell)
    {
        return cell?.ToString() ?? "<null>";
    }
}
