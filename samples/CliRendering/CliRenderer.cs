using Kusto.Language.Parsing;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using KustoLoco.Rendering.ScottPlot;
using NotNullStrings;
using ScottPlot;
using Spectre.Console;
using System.Diagnostics;
using NetTopologySuite.Triangulate;

namespace CliRendering;

/// <summary>
///     renders a KustoQueryResult to the console using Spectre.Console or via a browser chart if requested.
/// </summary>
public class CliRenderer
{
    private readonly string _renderPreference;

    /// <summary>
    ///     renders a KustoQueryResult to the console using Spectre.Console or via a browser chart if requested.
    /// </summary>
    /// <param name="renderPreference">
    ///     The render preference can be "sixel" or "browser".
    /// </param>
    public CliRenderer(string renderPreference)
    {
        _renderPreference = renderPreference;
    }

    /// <summary>
    ///     Print the error in red if it exists.
    /// </summary>
    /// <remarks>
    ///     return true if there was an error
    /// </remarks>
    public static bool DisplayError(string error)
    {
        if (error.IsBlank()) return false;
        AnsiConsole.MarkupLineInterpolated($"[red]{error}[/]");
        return true;
    }

    /// <summary>
    ///     Renders the results of a Kusto query as a formatted table in the console.
    /// </summary>
    /// <remarks>
    ///     This method uses Spectre.Console to render the table. Each column in the table corresponds to
    ///     a column in the query result, and each row represents a data row from the query result. Null values in cells are
    ///     displayed as "<null>".
    /// </remarks>
    private static void RenderTable(KustoQueryResult queryResult)
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
        return;

        // Helper function to convert a cell to a string, handling nulls
        string CellToString(object? cell)
        {
            return cell?.ToString() ?? "<null>";
        }
    }
    /// <summary>
    /// Display the query results in the format requested by the user
    /// </summary>
    /// <param name="queryResult"></param>
    public void DisplayQueryResult(KustoQueryResult queryResult)
    {
        // if we got an error display it and skip rendering an empty table
        if (DisplayError(queryResult.Error))
            return;

        if (queryResult.IsChart)
            RenderToChart(queryResult);
        else
            RenderTable(queryResult);
    }

    private void RenderToChart(KustoQueryResult queryResult)
    {
        if (_renderPreference=="sixel")
        {
            var sixel =ScottPlotKustoResultRenderer.RenderToSixelWithPad(queryResult, new KustoSettingsProvider(), 5);
            AnsiConsole.WriteLine(sixel);
        }
        else if (_renderPreference=="browser")
        {
            //render the results as a chart if we were asked to do that
            KustoResultRenderer.RenderChartInBrowser(queryResult);
        }
        else if (_renderPreference == "png")
        {
          RenderImageFormat(queryResult,_renderPreference,ImageFormat.Png);
        }
        else if (_renderPreference == "bmp")
        {
            RenderImageFormat(queryResult, _renderPreference, ImageFormat.Bmp);
        }
        else if (_renderPreference == "jpeg")
        {
            RenderImageFormat(queryResult, _renderPreference, ImageFormat.Jpeg);
        }

        else
            DisplayError($"Invalid display preference - use one of {AllowedRenderPreferences}");
      
    }

    private void RenderImageFormat(KustoQueryResult queryResult, string extension, ImageFormat format)
    {
        var imageBytes = ScottPlotKustoResultRenderer.RenderToImage(queryResult, format, 800, 600, new KustoSettingsProvider());
        var f = Path.ChangeExtension(Path.GetTempFileName(), extension);
        File.WriteAllBytes(f, imageBytes);
        Process.Start(new ProcessStartInfo { FileName = f, UseShellExecute = true });
    }

    private const string AllowedRenderPreferences = "sixel,browser,png,bmp,jpeg";
    public static bool ValidateRenderingPreference(string renderPreference)
    {
        renderPreference = renderPreference.ToLowerInvariant();
        var allowed = AllowedRenderPreferences.Tokenize(",");
        if (allowed.Any(a => a == renderPreference))
            return true;
        DisplayError($"Invalid display preference - use one of {AllowedRenderPreferences}");
        return false;
    }

    public static string GetValidatedRenderingPreference(string[] args, int minLength)
    {
        var renderingPreference = "sixel";
        if (args.Length >= minLength)
        {
            renderingPreference = args.First();
            if (!CliRenderer.ValidateRenderingPreference(renderingPreference))
                return string.Empty;
        }

        return renderingPreference;
    }

    public const string PreferencesHelp = """
        - sixel causes charts to be rendered as sixels (windows terminal _preview_ is required) and 
        - browser causes the chart to be rendered to an html file which will be opened 
          with the browser.  
        - png,bmp, and jpeg cause the chart to be rendered as an image file in the specified 
          format and opened with the defaul image-viewer.
        
        The default value is 'sixel';
        """;

    public static void DisplayPrompt(string prompt)
    {
        AnsiConsole.MarkupInterpolated($"[green]{prompt}[/]");
    }
}
