using System.Text.RegularExpressions;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine.Commands;
using NotNullStrings;

namespace Lokql.Engine;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local setters are
// required on Options properties by CommandLineParser library

/// <summary>
///     A repl for exploring kusto tables
/// </summary>
/// <remarks>
///     This class is used as the heart of the CLI and UI version of "lokql"  As well as providing demand-based loading of
///     tables,
///     it also provides a number of commands to interact with the data, such as saving, loading, rendering and
///     materializing tables.
///     Query results can also be rendered as html charts using the "render" command.
/// </remarks>
public class InteractiveTableExplorer
{
    private readonly CommandProcessor _commandProcessor;
    private readonly KustoQueryContext _context;

    public readonly ITableAdaptor _loader;
    public readonly IKustoConsole _outputConsole;
    public readonly KustoSettingsProvider Settings;

    public DisplayOptions _currentDisplayOptions = new(10);

    public InteractiveTableExplorer(IKustoConsole outputConsole, ITableAdaptor loader, KustoSettingsProvider settings,
        CommandProcessor commandProcessor)
    {
        _outputConsole = outputConsole;
        _loader = loader;
        Settings = settings;
        _commandProcessor = commandProcessor;
        _context = KustoQueryContext.CreateWithDebug(outputConsole, settings);
        _context.SetTableLoader(_loader);
        _prevResult = KustoQueryResult.Empty;
        LokqlSettings.Register(Settings);
    }

    public KustoQueryResult _prevResult { get; set; }


    public KustoQueryContext GetCurrentContext()
    {
        return _context;
    }

    public void ShowResultsToConsole(KustoQueryResult result, int start, int maxToDisplay)
    {
        _outputConsole.ForegroundColor = ConsoleColor.Green;


        var prefs = new KustoFormatter.DisplayPreferences(_outputConsole.WindowWidth, start, maxToDisplay);
        _outputConsole.WriteLine(KustoFormatter.Tabulate(result, prefs));

        if (maxToDisplay < result.RowCount)
            Warn(
                $"Display was truncated to first {maxToDisplay} of {result.RowCount}.  Use '.display -m <n>' to change this behaviour");
    }

    private void DisplayResults(KustoQueryResult result)
    {
        if (result.Error.IsNotBlank())
        {
            ShowError(result.Error);
        }
        else
        {
            _outputConsole.ForegroundColor = ConsoleColor.Green;
            if (result.RowCount == 0)
            {
                Warn("No results");
            }
            else
            {
                var max = _currentDisplayOptions.MaxToDisplay;
                ShowResultsToConsole(result, 0, max);
            }

            Warn($"Query took {(int)result.QueryDuration.TotalMilliseconds}ms");
        }
    }

    public string Interpolate(string query)
    {
        string rep(Match m)
        {
            var term = m.Groups[1].Value;
            return Settings.TrySubstitute(term);
        }

        return Regex.Replace(query, @"\$(\w+)", rep);
    }

    public async Task RunInput(string query)
    {
        var breaker = new BlockBreaker(query);
        var sequence = new BlockSequence(breaker.Blocks);

        while (!sequence.Complete) await RunInput(sequence);
    }

    public async Task RunInput(BlockSequence blocks)
    {
        var query = blocks.Next();
        query = Interpolate(query)
            .Trim();
        //support comments
        if (query.StartsWith("#") | query.IsBlank()) return;

        try
        {
            if (query.StartsWith("."))
            {
                await _commandProcessor.RunInternalCommand(this, query.Substring(1), blocks);
                return;
            }

            var result = await GetCurrentContext().RunQuery(query);
            if (result.Error.Length == 0)
                _prevResult = result;

            DisplayResults(result);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    public void InjectResult(KustoQueryResult result)
    {
        _prevResult = result;
    }

    private void ShowError(string message)
    {
        _outputConsole.ForegroundColor = ConsoleColor.DarkRed;
        _outputConsole.WriteLine("Error:");
        _outputConsole.WriteLine(message);
    }


    public static string ToFullPath(string file, string folder, string extension)
    {
        var path = Path.IsPathRooted(file)
            ? file
            : Path.Combine(folder, file);
        if (!Path.HasExtension(path))
            path = Path.ChangeExtension(path, extension);
        return path;
    }

    public void SetWorkingPaths(string containingFolder)
    {
        _loader.SetDataPaths(containingFolder);
        Settings.Set(LokqlSettings.ScriptPath.Name, containingFolder);
        Settings.Set(LokqlSettings.QueryPath.Name, containingFolder);
    }

    public void Info(string s)
    {
        _outputConsole.ForegroundColor = ConsoleColor.Yellow;
        _outputConsole.WriteLine(s);
    }

    #region internal commands

    public void Warn(string s)
    {
        _outputConsole.Warn(s);
    }

    #endregion

    public readonly record struct DisplayOptions(int MaxToDisplay);
}
