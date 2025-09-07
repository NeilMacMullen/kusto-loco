using Kusto.Language.Symbols;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Evaluation.BuiltIns;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using KustoLoco.PluginSupport;
using Lokql.Engine.Commands;
using NotNullStrings;

namespace Lokql.Engine;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local setters are
// required on Options properties by CommandLineParser library

/// <summary>
///     A repl for exploring kusto tables
/// </summary>
/// <remarks>
///     This class is used as the heart of the CLI and UI version of "lokql"
///     As well as providing demand-based loading of tables, it also provides
///     a number of commands to interact with the data, such as saving,
///     loading, rendering and materializing tables.
/// </remarks>
public class InteractiveTableExplorer
{
    public readonly CommandProcessor _commandProcessor;
    private readonly KustoQueryContext _context;
    public readonly ITableAdaptor _loader;
    private readonly MacroRegistry _macros;
    public readonly IKustoConsole _outputConsole;
    public readonly IResultRenderingSurface _renderingSurface;
    public readonly ResultHistory _resultHistory;
    public readonly KustoSettingsProvider Settings;


    public InteractiveTableExplorer(IKustoConsole outputConsole, KustoSettingsProvider settings,
        CommandProcessor commandProcessor, IResultRenderingSurface renderingSurface,
        Dictionary<FunctionSymbol, ScalarFunctionInfo> additionalFunctions)
    {
        _outputConsole = outputConsole;
        Settings = settings;
        _commandProcessor = commandProcessor;
        _renderingSurface = renderingSurface;
        _context = KustoQueryContext.CreateWithDebug(outputConsole, settings);
        _context.AddFunctions(additionalFunctions);
        _loader = new StandardFormatAdaptor(settings, _outputConsole);
        _context.SetTableLoader(_loader);
        _resultHistory = new ResultHistory();
        _macros = new MacroRegistry();
        LokqlSettings.Register(Settings);
    }


    /// <summary>
    ///     Used to create a clone
    /// </summary>
    private InteractiveTableExplorer(IKustoConsole outputConsole, KustoSettingsProvider settings,
        CommandProcessor commandProcessor, IResultRenderingSurface renderingSurface, ResultHistory history,
        MacroRegistry macros,
        ITableAdaptor loader, KustoQueryContext context)
    {
        _outputConsole = outputConsole;
        Settings = settings;
        _loader = new StandardFormatAdaptor(settings, _outputConsole);
        _commandProcessor = commandProcessor;
        _renderingSurface = renderingSurface;
        _context = context;
        _macros = macros;
        _resultHistory = history;
    }


    public IReportTarget ActiveReport { get; private set; } = new HtmlReport(string.Empty);

    public IResultRenderingSurface GetRenderingSurface() => _renderingSurface;

    public void AddMacro(MacroDefinition macro) => _macros.AddMacro(macro);

    public KustoQueryContext GetCurrentContext() => _context;

    public SchemaLine[] GetSchema()
    {
        var commandSchema = _commandProcessor.GetRegisteredSchema();

        var dynamicSchema = _context
            .Tables()
            .SelectMany(t => t.Type.Columns.Select(c =>
                new SchemaLine(string.Empty, NameEscaper.EscapeIfNecessary(t.Name),
                    NameEscaper.EscapeIfNecessary(c.Name),c.Type.Name)));

        return commandSchema.Concat(dynamicSchema).ToArray();
    }

    public void ShowResultsToConsole(KustoQueryResult result, int start, int maxToDisplay)
    {
        if (maxToDisplay == 0)
            return;
        _outputConsole.ForegroundColor = ConsoleColor.Green;


        var prefs = new KustoFormatter.DisplayPreferences(_outputConsole.WindowWidth, start, maxToDisplay);
        _outputConsole.WriteLine(KustoFormatter.Tabulate(result, prefs));

        if (maxToDisplay < result.RowCount)
            Warn(
                $"Display was truncated to first {maxToDisplay} of {result.RowCount}.  Use '.display -m <n>' to change this behaviour");
    }

    public void DisplayResultsToConsole(KustoQueryResult result)
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
                var max = Settings.GetIntOr("console.maxrows", 0);
                ShowResultsToConsole(result, 0, max);
            }

            Warn($"{result.RowCount} rows in {(int)result.QueryDuration.TotalMilliseconds}ms");
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
        query = Interpolate(query).Trim();

        //support comments
        if (query.StartsWith("#") || query.IsBlank()) return;

        try
        {
            if (query.StartsWith("."))
            {
                await _commandProcessor.RunInternalCommand(this, query.Substring(1), blocks);
                return;
            }

            var result = await GetCurrentContext().RunQuery(query);
            await _renderingSurface.RenderToDisplay(result);
            _resultHistory.Push(result);

            DisplayResultsToConsole(result);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }

    public async Task InjectResult(KustoQueryResult result)
    {
        _resultHistory.Push(result);
        await _renderingSurface.RenderToDisplay(result);
        DisplayResultsToConsole(result);
    }

    /// <summary>
    /// Print an error to the console
    /// </summary>
    private void ShowError(string message)
    {
        
        _outputConsole.Error(message);
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

    /// <summary>
    /// Print info to the console
    /// </summary>
    public void Info(string s)
    {
        _outputConsole.Info(s);
    }

    #region internal commands

    /// <summary>
    /// Print a warning to the console
    /// </summary>
    public void Warn(string s) => _outputConsole.Warn(s);

    #endregion

    /// <summary>
    ///     Adds a layer of settings to the setting interpolation stack
    /// </summary>
    /// <remarks>
    ///     Primarily used by the Macro mechanism to avoid name-clashes for parameters
    /// </remarks>
    public void PushSettingLayer(KustoSettingsProvider settings) => Settings.AddLayer(settings);

    /// <summary>
    ///     Pops the top layer of settings from the setting interpolation stack
    /// </summary>
    /// <remarks>
    /// Inverse of PushSettingsLayer
    /// </remarks>
    public void PopSettingLayer() => Settings.Pop();

    public MacroDefinition GetMacro(string oName) => _macros.GetMacro(oName);
    public IEnumerable<MacroDefinition> ListMacros() => _macros.List();

    public void StartNewReport(IReportTarget report) => ActiveReport = report;

    public KustoQueryResult GetPreviousResult() => _resultHistory.MostRecent;

    public KustoQueryResult GetResult(string name) => _resultHistory.Fetch(name);

    public InteractiveTableExplorer ShareWithNewSurface(IResultRenderingSurface renderingSurface) =>
        new(_outputConsole, Settings, _commandProcessor, renderingSurface,
            _resultHistory, _macros, _loader, _context);

    /// <summary>
    ///     Interpolates settings in the supplied text
    /// </summary>
    /// <remarks>
    ///     a query such as "project $col" can be transformed into "project Id"
    /// </remarks>
    public string Interpolate(string query) => BlockInterpolator.Interpolate(query, Settings);
}
