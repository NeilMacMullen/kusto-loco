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
    public readonly BlockInterpolator _interpolator;

    public readonly ITableAdaptor _loader;
    private readonly MacroRegistry _macros = new();
    public readonly IKustoConsole _outputConsole;
    public readonly KustoSettingsProvider Settings;

    public DisplayOptions _currentDisplayOptions = new(10);
  
    public IReportTarget ActiveReport { get; private set; } = new HtmlReport(string.Empty);


    public Func<KustoQueryResult, Task> GetRenderCallback = result => Task.CompletedTask; 
    public InteractiveTableExplorer(IKustoConsole outputConsole, ITableAdaptor loader, KustoSettingsProvider settings,
        CommandProcessor commandProcessor)
    {
        _outputConsole = outputConsole;
        _loader = loader;
        Settings = settings;
        _interpolator = new BlockInterpolator(settings);
        _commandProcessor = commandProcessor;
        _context = KustoQueryContext.CreateWithDebug(outputConsole, settings);
        _context.SetTableLoader(_loader);
      LokqlSettings.Register(Settings);
    }

    public ResultHistory _resultHistory = new ResultHistory();


    public void AddMacro(MacroDefinition macro)
    {
        _macros.AddMacro(macro);
    }

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


    public async Task RunInput(string query)
    {
        var breaker = new BlockBreaker(query);
        var sequence = new BlockSequence(breaker.Blocks);
        await RunSequence(sequence);
    }

    public async Task RunSequence(BlockSequence sequence)
    {
        while (!sequence.Complete)
            await RunNextBlock(sequence);
    }

    public async Task RunNextBlock(BlockSequence blocks)
    {
        var query = blocks.Next();
        query = _interpolator.Interpolate(query)
            .Trim();

        //support comments
        if (query.StartsWith("#") || query.StartsWith("//") || query.IsBlank()) return;

        try
        {
            if (query.StartsWith("."))
            {
                await _commandProcessor.RunInternalCommand(this, query.Substring(1), blocks);
                return;
            }

            var result = await GetCurrentContext().RunQuery(query);
            if (result.Error.Length == 0)
            {
                var renderTask = GetRenderCallback(result);
                await renderTask;
            }

           _resultHistory.Push(result);

            DisplayResults(result);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    public void InjectResult(KustoQueryResult result)
    {
        _resultHistory.Push(result);
    }

    private void ShowError(string message)
    {
        _outputConsole.ForegroundColor = ConsoleColor.Red;
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

    /// <summary>
    ///     Adds a layer of settings to the setting interpolation stack
    /// </summary>
    /// <remarks>
    ///     Primarily used by the Macro mechanism to avoid name-clashes for parameters
    /// </remarks>
    public void PushSettingLayer(KustoSettingsProvider settings)
    {
        _interpolator.PushSettings(settings);
    }

    /// <summary>
    ///     Pops the top layer of settings from the setting interpolation stack
    /// </summary>
    public void PopSettingLayer()
    {
        _interpolator.PopSettings();
    }

    public MacroDefinition GetMacro(string oName)
    {
        return _macros.GetMacro(oName);
    }

    public readonly record struct DisplayOptions(int MaxToDisplay);

    public void StartNewReport(IReportTarget report)
    {
        ActiveReport = report;
    }
    #region image support
    //Ugh this is horrible - figure out a better way
    private byte[] _image = [];
    public byte[] GetImageBytes()
    {
        return _image;
    }

    public void SetImage(byte[] getBuffer)
    {
        _image = getBuffer;
    }
    #endregion

    public KustoQueryResult GetPreviousResult()
    {
        return _resultHistory.MostRecent;
    }
}


public class ResultHistory
{
    private Dictionary<string,KustoQueryResult> _results= new ();
    public KustoQueryResult _mostRecent = KustoQueryResult.Empty;

    public void Push(KustoQueryResult result)
    {
        _mostRecent=result;
    }

    public void Save(string name)
    {
        _results[name] = _mostRecent;
    }

    public KustoQueryResult Fetch(string name) =>
        _results.TryGetValue(name, out var found)
        ? found
        : KustoQueryResult.Empty;

    public KustoQueryResult MostRecent => _mostRecent;

}
