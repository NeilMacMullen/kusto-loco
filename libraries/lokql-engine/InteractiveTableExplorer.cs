using System.Collections.Specialized;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text.RegularExpressions;
using AppInsightsSupport;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using NLog;
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
    private readonly KustoQueryContext _context;

    private readonly ITableAdaptor _loader;
    private readonly IKustoConsole _outputConsole;
    public readonly KustoSettingsProvider _settings;

    private DisplayOptions _currentDisplayOptions = new(10);

    public InteractiveTableExplorer(IKustoConsole outputConsole, ITableAdaptor loader, KustoSettingsProvider settings)
    {
        _outputConsole = outputConsole;
        _loader = loader;
        _settings = settings;
        _context = KustoQueryContext.CreateWithDebug(outputConsole, settings);
        _context.SetTableLoader(_loader);
        _prevResult = KustoQueryResult.Empty;
        LokqlSettings.Register(_settings);
    }

    public KustoQueryResult _prevResult { get; private set; }


    private KustoQueryContext GetCurrentContext()
    {
        return _context;
    }

    private void ShowResultsToConsole(KustoQueryResult result, int start, int maxToDisplay)
    {
        _outputConsole.ForegroundColor = ConsoleColor.Green;


        var prefs = new KustoFormatter.DisplayPreferences(_outputConsole.WindowWidth, start, maxToDisplay);
        _outputConsole.WriteLine(KustoFormatter.Tabulate(result, prefs));

        if (maxToDisplay < result.RowCount)
            Warn(
                $"Display was truncated to first {maxToDisplay} of {result.RowCount}.  Use '.display -m <n>' to change this behaviour");
    }

    private void DisplayResults(KustoQueryResult result, bool autoRender)
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
                //auto render

                if (autoRender && result.Visualization != VisualizationState.Empty)
                    RenderCommand.Run(this, new RenderCommand.Options());
            }

            Warn($"Query took {(int)result.QueryDuration.TotalMilliseconds}ms");
        }
    }

    public async Task RunInteractive(string initialScript)
    {
        _outputConsole.WriteLine("Use '.help' to list commands");

        if (initialScript.IsNotBlank()) await RunInput($".run {initialScript}", true);

        while (true)
        {
            _outputConsole.ForegroundColor = ConsoleColor.Blue;
            _outputConsole.Write("KQL> ");
            var query = _outputConsole.ReadLine();
            await RunInput(query, true);
        }
    }

    private async Task ExecuteAsync(string line)
    {
        _outputConsole.ForegroundColor = ConsoleColor.Blue;

        _outputConsole.WriteLine($"KQL> {line}");
        await RunInput(line, true);
    }


    public string Interpolate(string query)
    {
        string rep(Match m)
        {
            var term = m.Groups[1].Value;
            return _settings.TrySubstitute(term);
        }

        return Regex.Replace(query, @"\$(\w+)", rep);
    }

    public async Task RunInput(string query, bool autoRender)
    {
        var breaker = new BlockBreaker(query);
        var sequence = new BlockSequence(breaker.Blocks);

        while (!sequence.Complete) await RunInput(sequence, autoRender);
    }

    public async Task RunInput(BlockSequence blocks, bool autoRender)
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
                var splitter = CommandLineStringSplitter.Instance;
                var tokens = splitter.Split(query.Substring(1)).ToArray();

                switch (tokens[0])
                {
                    case "tables":
                        break;
                    default:
                        await RunInternalCommand(tokens);
                        return;
                }
            }


            var result = await GetCurrentContext().RunQuery(query);
            if (result.Error.Length == 0)
                _prevResult = result;

            DisplayResults(result, autoRender);
            return;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
    }


    private void ShowError(string message)
    {
        _outputConsole.ForegroundColor = ConsoleColor.DarkRed;
        _outputConsole.WriteLine("Error:");
        _outputConsole.WriteLine(message);
    }


    private static string ToFullPath(string file, string folder, string extension)
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
        _settings.Set(LokqlSettings.ScriptPath.Name, containingFolder);
        _settings.Set(LokqlSettings.QueryPath.Name, containingFolder);
    }


    internal readonly record struct DisplayOptions(int MaxToDisplay);

    #region internal commands

    private async Task RunInternalCommand(string[] args)
    {
        var textWriter = new StringWriter();
        await StandardParsers.CreateWithHelpWriter(textWriter)
                .ParseArguments(args,
                    typeof(ExitCommand.Options),
                    typeof(SaveCommand.Options),
                    typeof(RenderCommand.Options),
                    typeof(LoadCommand.Options),
                    typeof(FormatCommand.Options),
                    typeof(RunScriptCommand.Options),
                    typeof(QueryCommand.Options),
                    typeof(SaveQueryCommand.Options),
                    typeof(MaterializeCommand.Options),
                    typeof(SynTableCommand.Options),
                    typeof(AllTablesCommand.Options),
                    typeof(ShowCommand.Options),
                    typeof(TestCommand.Options),
                    typeof(FileFormatsCommand.Options),
                    typeof(SetCommand.Options),
                    typeof(ListSettingsCommand.Options),
                    typeof(ListSettingDefinitionsCommand.Options),
                    typeof(AppInsightsCommand.Options),
                    typeof(PivotCommand.Options)
                )
                .WithParsed<PivotCommand.Options>(o => PivotCommand.Run(this, o))
                .WithParsed<MaterializeCommand.Options>(o => MaterializeCommand.Run(this, o))
                .WithParsed<RenderCommand.Options>(o => RenderCommand.Run(this, o))
                .WithParsed<AllTablesCommand.Options>(o => AllTablesCommand.Run(this, o))
                .WithParsed<ExitCommand.Options>(o => ExitCommand.Run(this, o))
                .WithParsed<FormatCommand.Options>(o => FormatCommand.Run(this, o))
                .WithParsed<SynTableCommand.Options>(o => SynTableCommand.Run(this, o))
                .WithParsedAsync<RunScriptCommand.Options>(o => RunScriptCommand.RunAsync(this, o))
                .WithParsedAsync<SaveQueryCommand.Options>(o => SaveQueryCommand.RunAsync(this, o))
                .WithParsedAsync<LoadCommand.Options>(o => LoadCommand.RunAsync(this, o))
                .WithParsedAsync<SaveCommand.Options>(o => SaveCommand.RunAsync(this, o))
                .WithParsedAsync<QueryCommand.Options>(o => QueryCommand.RunAsync(this, o))
                .WithParsedAsync<ShowCommand.Options>(o => ShowCommand.RunAsync(this, o))
                .WithParsedAsync<TestCommand.Options>(o => TestCommand.RunAsync(this, o))
                .WithParsedAsync<FileFormatsCommand.Options>(o => FileFormatsCommand.RunAsync(this, o))
                .WithParsedAsync<SetCommand.Options>(o => SetCommand.RunAsync(this, o))
                .WithParsedAsync<ListSettingsCommand.Options>(o => ListSettingsCommand.RunAsync(this, o))
                .WithParsedAsync<ListSettingDefinitionsCommand.Options>(o =>
                    ListSettingDefinitionsCommand.RunAsync(this, o))
                .WithParsedAsync<AppInsightsCommand.Options>(o => AppInsightsCommand.RunAsync(this, o))
            ;
        _outputConsole.Info(textWriter.ToString());
    }


    public static class RunScriptCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var scriptFolder = exp._settings.Get(LokqlSettings.ScriptPath);

            var filename = ToFullPath(o.File, scriptFolder, ".dfr");

            exp.Info($"Loading script '{filename}'..");
            var text = await File.ReadAllTextAsync(filename);
            await exp.RunInput(text, false);
        }

        [Verb("run", aliases: ["script", "r"],
            HelpText = "run a script")]
        internal class Options
        {
            [Value(0, HelpText = "Name of script", Required = true)]
            public string File { get; set; } = string.Empty;
        }
    }

    private void Info(string s)
    {
        _outputConsole.ForegroundColor = ConsoleColor.Yellow;
        _outputConsole.WriteLine(s);
    }

    public static class QueryCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var queryFolder = exp._settings.Get(LokqlSettings.QueryPath);
            var filename = ToFullPath(o.File, queryFolder, ".csl");
            exp.Info($"Fetching query '{filename}'");
            var query = await File.ReadAllTextAsync(filename);
            await exp.ExecuteAsync($"{o.Prefix}{query}");
        }

        [Verb("query", aliases: ["q"],
            HelpText = "run a multi-line query from a file")]
        internal class Options
        {
            [Value(0, HelpText = "Name of queryFile", Required = true)]
            public string File { get; set; } = string.Empty;

            [Option('p', HelpText = "prefix to add to script before running")]
            public string Prefix { get; set; } = string.Empty;
        }
    }

    public static class SaveQueryCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var queryFolder = exp._settings.Get(LokqlSettings.QueryPath);
            var filename = ToFullPath(o.File, queryFolder, ".csl");
            exp.Info($"Saving query as '{filename}'");
            var q = exp._prevResult.Query;
            if (!o.NoSplit)
                q = q
                        .Tokenize("|")
                        .JoinString($"{Environment.NewLine}| ")
                        .Tokenize(";")
                        .JoinString($";{Environment.NewLine}")
                    ;

            var text = $@"//{o.Comment}
    {q}
    ";
            await File.WriteAllTextAsync(filename, text);
        }

        [Verb("savequery", aliases: ["sq"],
            HelpText = "save the previous query to a file so you can reuse it")]
        internal class Options
        {
            [Value(0, HelpText = "Name of queryFile", Required = true)]
            public string File { get; set; } = string.Empty;

            [Option('c', HelpText = "Adds a comment")]
            public string Comment { get; set; } = string.Empty;

            [Option(HelpText = "Prevents query being split into multiple lines at '|' and ';'")]
            public bool NoSplit { get; set; }
        }
    }

    public static class SaveCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            await exp._loader.SaveResult(exp._prevResult, o.File);
        }

        [Verb("save", aliases: ["sv"], HelpText = "save last results to file")]
        internal class Options
        {
            [Value(0, HelpText = "Name of file", Required = true)]
            public string File { get; set; } = string.Empty;
        }
    }

    public static class FileFormatsCommand
    {
        internal static Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var formats = exp._loader.GetSupportedAdaptors()
                .Select(f => $"{f.Name} ({f.Extensions.JoinString(", ")})")
                .JoinAsLines();
            exp.Info(formats);
            return Task.CompletedTask;
        }

        [Verb("formats", aliases: ["fmts"], HelpText = "list supported file formats for save/load")]
        internal class Options
        {
        }
    }

    public static class TestCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            await Task.Run(() =>
            {
                for (var i = 0; i < 100; i++)
                {
                    exp.Info($"Line {i}");
                    var t = Stopwatch.StartNew();
                    while (t.ElapsedMilliseconds < 100) ;
                }
            });
        }

        [Verb("test", HelpText = "test")]
        internal class Options
        {
        }
    }

    public static class ShowCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            exp.ShowResultsToConsole(exp._prevResult, o.Offset, o.NumToShow);
            await Task.CompletedTask;
        }

        [Verb("show", HelpText = "show the last results again")]
        internal class Options
        {
            [Value(0, HelpText = "Rows to show")] public int NumToShow { get; set; } = 50;


            [Value(1, HelpText = "Offset to show data from")]
            public int Offset { get; set; } = 0;
        }
    }

    public static class RenderCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            var fileName = Path.ChangeExtension(o.File.OrWhenBlank(Path.GetTempFileName()), "html");
            var result = exp._prevResult;
            var renderer = new KustoResultRenderer(exp._settings);
            var text = renderer.RenderToHtml(result);
            File.WriteAllText(fileName, text);
            exp.Info($"Saved chart as {fileName}");
            if (!o.SaveOnly)
                Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        }

        [Verb("render", aliases: ["ren"], HelpText = "render last results as html and opens with browser")]
        internal class Options
        {
            [Value(0, HelpText = "Name of file")] public string File { get; set; } = string.Empty;

            [Option("saveOnly", HelpText = "just save the file without opening in the browser")]
            public bool SaveOnly { get; set; }
        }
    }

    public static class PivotCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            var result = exp._prevResult;
            var ods = new List<OrderedDictionary>();
            var columns = result.ColumnDefinitions();
            foreach (var row in result.EnumerateRows())
                for (var i = o.Keep; i < row.Length; i++)
                {
                    var od = new OrderedDictionary();

                    for (var k = 0; k < o.Keep; k++)
                        od[columns[k].Name] = row[k];

                    od["Column"] = columns[i].Name;
                    od["Data"] = row[i];
                    ods.Add(od);
                }

            var builder = TableBuilder.FromOrderedDictionarySet(o.As, ods);
            exp.GetCurrentContext().AddTable(builder);
        }

        [Verb("pivot", HelpText = "pivots columns into rows")]
        internal class Options
        {
            [Value(0, Required = true, HelpText = "Name of table")]
            public string As { get; set; } = string.Empty;

            [Option(HelpText = "Columns before first pivoted column")]
            public int Keep { get; set; } = 1;
        }
    }

    public static class MaterializeCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            exp.GetCurrentContext().MaterializeResultAsTable(exp._prevResult, o.As);
            exp.Info($"Table '{o.As}' now available");
        }

        [Verb("materialize", aliases: ["materialise", "mat"],
            HelpText = "save last results back into context as a table")]
        internal class Options
        {
            [Value(0, Required = true, HelpText = "Name of table")]
            public string As { get; set; } = string.Empty;
        }
    }


    public static class AllTablesCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            var context = exp._context;
            var tableNames = context.TableNames
                .Select(t => $"['{t}']")
                .JoinAsLines();
            exp._outputConsole.WriteLine(tableNames);
        }

        [Verb("listtables", aliases: ["ls", "alltables", "at"],
            HelpText = "Lists all available tables in the global context")]
        internal class Options
        {
        }
    }

    public static class LoadCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var tableName = o.As.OrWhenBlank(Path.GetFileNameWithoutExtension(o.File));
            //remove table if it already exists
            if (exp._context.HasTable(tableName))
            {
                if (o.Force)
                {
                    exp._context.RemoveTable(tableName);
                }
                else
                {
                    exp.Warn($"Table '{tableName}' already exists.  Use '.load -f' to force reload");
                    return;
                }
            }


            var success = await exp._loader.LoadTable(exp._context, o.File, tableName);
            if (!success) exp.Warn($"Unable to load '{o.File}'");
        }

        [Verb("load", aliases: ["ld"],
            HelpText = "loads a data file")]
        internal class Options
        {
            [Value(0, HelpText = "Name of file", Required = true)]
            public string File { get; set; } = string.Empty;

            [Value(1, HelpText = "Name of table (defaults to name of file)")]
            public string As { get; set; } = string.Empty;

            [Option('f', "force", HelpText = "Force reload")]
            public bool Force { get; set; }
        }
    }

    public static class AppInsightsCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var ai = new ApplicationInsightsLogLoader(exp._settings, exp._outputConsole);
            var result = await ai.LoadTable(o.Rid, o.Query, TimeSpan.FromDays(7));
            exp._prevResult = result;
        }

        [Verb("appinsights", aliases: ["ai"],
            HelpText = "Runs a query against an application insights log-set")]
        internal class Options
        {
            [Value(0, HelpText = "resourceId", Required = true)]
            public string Rid { get; set; } = string.Empty;

            [Value(1, HelpText = "query")] public string Query { get; set; } = string.Empty;
        }
    }

    public static class SetCommand
    {
        internal static Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            exp._settings.Set(o.Name, o.Value);
            exp.Info($"Set {o.Name} to {o.Value}");
            return Task.CompletedTask;
        }

        [Verb("set", HelpText = "sets a setting value")]
        internal class Options
        {
            [Value(0, HelpText = "setting name", Required = true)]
            public string Name { get; set; } = string.Empty;

            [Value(1, HelpText = "Value of setting (omit to remove)")]
            public string Value { get; set; } = string.Empty;
        }
    }

    public static class ListSettingsCommand
    {
        internal static Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var settings = exp._settings.Enumerate()
                .Where(s => s.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(s => s.Name);

            var str = Tabulator.Tabulate(settings, "Name|Value", s => s.Name, s => s.Value);
            exp.Info(str);
            return Task.CompletedTask;
        }

        [Verb("settings", HelpText = "lists all settings")]
        internal class Options
        {
            [Value(0, HelpText = "match substring", Required = false)]
            public string Match { get; set; } = string.Empty;
        }
    }


    public static class ListSettingDefinitionsCommand
    {
        internal static Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var defs = exp._settings.GetDefinitions()
                .Where(d => d.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
                .OrderBy(d => d.Name);

            var str = Tabulator.Tabulate(defs, "Name|Description|Default", d => d.Name, d => d.Description,
                d => d.DefaultValue);
            exp.Info(str);
            return Task.CompletedTask;
        }

        [Verb("settingdefinitions", HelpText = "lists all setting definitions")]
        internal class Options
        {
            [Value(0, HelpText = "match substring", Required = false)]
            public string Match { get; set; } = string.Empty;
        }
    }

    public void Warn(string s)
    {
        _outputConsole.Warn(s);
    }


    public static class SynTableCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            exp.Info($"Creating synonym for table {o.CurrentName} as {o.As} ...");

            exp.GetCurrentContext().ShareTable(o.CurrentName, o.As);
        }

        [Verb("synonym", aliases: ["syn", "alias"], HelpText = "provides a synonym for a table")]
        internal class Options
        {
            [Value(0, HelpText = "table name", Required = true)]
            public string CurrentName { get; set; } = string.Empty;

            [Value(1, HelpText = "Synonym", Required = true)]
            public string As { get; set; } = string.Empty;
        }
    }

    private static class FormatCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            if (o.Max > 0)
                exp._currentDisplayOptions = exp._currentDisplayOptions with { MaxToDisplay = o.Max };
            exp.Info($"Set: {exp._currentDisplayOptions}");
        }

        [Verb("display", aliases: ["d"], HelpText = "change the output format")]
        internal class Options
        {
            [Option('m', HelpText = "Maximum number of items to display in console")]
            public int Max { get; set; } = -1;
        }
    }


    private static class ExitCommand
    {
        internal static void Run(InteractiveTableExplorer exp, Options o)
        {
            exp.Warn("Exiting...");
            Environment.Exit(0);
        }

        [Verb("exit", aliases: ["quit"], HelpText = "Exit application")]
        internal class Options;
    }

    #endregion
}

public static class LokqlSettings
{
    public static readonly KustoSettingDefinition ScriptPath = new("lokql.scriptpath",
        "location of script files", @"C:\kusto", nameof(String));

    public static readonly KustoSettingDefinition QueryPath = new("lokql.querypath",
        "location of query files", @"C:\kusto", nameof(String));

    public static void Register(KustoSettingsProvider settings)
    {
        settings.Register(ScriptPath);
        settings.Register(QueryPath);
    }
}
