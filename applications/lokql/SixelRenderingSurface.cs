using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using Lokql.Engine.Commands;
using NotNullStrings;
using Spectre.Console;

internal class SixelRenderingSurface(KustoSettingsProvider settings)
    : IResultRenderingSurface
{
    private const string SettingName = "table.maxrows";
    private const int DefaultRows = 100;

    public async Task RenderToDisplay(KustoQueryResult result)
    {
        await Task.CompletedTask;
        if (result.Error.IsNotBlank())
            return;

        if (result.IsChart)
        {
            var str = ScottPlotKustoResultRenderer.RenderToSixelWithPad(result,
                settings, 3);
            Console.WriteLine(str);
        }
        else
        {
            RenderTable(result);
        }
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight) =>
        throw new NotImplementedException();

    private void RenderTable(KustoQueryResult queryResult)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        //display the results as a pretty Spectre.Console table
        var table = new Table();

        // Add columns with header names
        foreach (var column in queryResult.ColumnDefinitions()) table.AddColumn(column.Name);
        var maxRows = settings.GetIntOr(SettingName, DefaultRows);
        // Add rows.  Note that cells could contain nulls in the general case
        foreach (var row in queryResult.EnumerateRows().Take(maxRows))
        {
            var rowCells = row.Select(CellToString).ToArray();
            table.AddRow(rowCells);
        }

        AnsiConsole.Write(table);
        if (maxRows < queryResult.RowCount)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            AnsiConsole.WriteLine($"Only {maxRows} of {queryResult.RowCount} shown.");
            AnsiConsole.WriteLine($"Set {SettingName} to see more");
        }

        return;

        // Helper function to convert a cell to a string, handling nulls
        string CellToString(object? cell)
        {
            return cell?.ToString() ?? "<null>";
        }
    }
}
