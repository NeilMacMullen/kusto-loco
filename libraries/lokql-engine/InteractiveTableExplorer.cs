using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.FileFormats;
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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly KustoQueryContext _context = new();

    private readonly FolderContext _folders;
    private readonly ITableAdaptor _loader;
    private readonly KustoSettings _settings;
    private readonly IConsole _outputConsole;
    private readonly StringBuilder _commandBuffer = new();

    private DisplayOptions _currentDisplayOptions = new(10);
    private KustoQueryResult _prevResult;

    public InteractiveTableExplorer(IConsole outputConsole, FolderContext folders, ITableAdaptor loader,KustoSettings settings)
    {
        _outputConsole = outputConsole;
        _loader = loader;
        _settings = settings;
        _context.SetTableLoader(_loader);
        _folders = folders;
        _prevResult = KustoQueryResult.Empty;
    }

   

    private KustoQueryContext GetCurrentContext()
    {
        return _context;
    }

    private void ShowResultsToConsole(KustoQueryResult result, int start, int maxToDisplay)
    {
        _outputConsole.SetForegroundColor(ConsoleColor.Green);


        var prefs = new KustoFormatter.DisplayPreferences(_outputConsole.WindowWidth, start, maxToDisplay);
        _outputConsole.WriteLine(KustoFormatter.Tabulate(result, prefs));

        if (maxToDisplay < result.RowCount)
            Warn(
                $"Display was truncated to first {maxToDisplay} of {result.RowCount}.  Use '.display --max' to change this behaviour");
    }

    private void DisplayResults(KustoQueryResult result, bool autoRender)
    {
        if (result.Error.IsNotBlank())
        {
            ShowError(result.Error);
        }
        else
        {
            _outputConsole.SetForegroundColor(ConsoleColor.Green);
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
            _outputConsole.SetForegroundColor(ConsoleColor.Blue);
            _outputConsole.Write("KQL> ");
            var query = _outputConsole.ReadLine();
            await RunInput(query, true);
        }
    }

    private async Task ExecuteAsync(string line)
    {
        _outputConsole.SetForegroundColor(ConsoleColor.Blue);

        _outputConsole.WriteLine($"KQL> {line}");
        await RunInput(line, true);
    }

    public async Task<KustoQueryResult> RunInput(string query, bool autoRender)
    {
        query = query.Trim();
        //support comments
        if (query.StartsWith("#") | query.IsBlank()) return KustoQueryResult.Empty;
        if (query.EndsWith("\\"))
        {
            _commandBuffer.Append(query.Substring(0, query.Length - 1) + " ");
            return KustoQueryResult.Empty;
        }

        if (query.EndsWith("|"))
        {
            _commandBuffer.Append(query);
            return KustoQueryResult.Empty;
        }

        _commandBuffer.Append(query);
        query = _commandBuffer.ToString().Trim();
        _commandBuffer.Clear();
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
                        return KustoQueryResult.Empty;
                }
            }


            var result = await GetCurrentContext().RunQuery(query);
            if (result.Error.Length == 0)
                _prevResult = result;

            DisplayResults(result, autoRender);
            return result;
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }


        return KustoQueryResult.Empty;
    }


    private void ShowError(string message)
    {
        _outputConsole.SetForegroundColor(ConsoleColor.DarkRed);
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

    public readonly record struct FolderContext(
        string DataFolder,
        string ScriptFolder,
        string QueryFolder);


    internal readonly record struct DisplayOptions(int MaxToDisplay);

    #region internal commands

    private Task RunInternalCommand(string[] args)
    {
        var textWriter = new ConsoleTextWriter(_outputConsole);
        return StandardParsers.CreateWithHelpWriter(textWriter)
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
                    typeof(ListSettingsCommand.Options)
                )
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
            ;
    }


    public static class RunScriptCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var filename = ToFullPath(o.File, exp._folders.ScriptFolder, ".dfr");

            exp.Info($"Loading '{filename}'..");
            var lines = await File.ReadAllLinesAsync(filename);
            foreach (var line in lines) await exp.ExecuteAsync(line);
        }

        [Verb("run", aliases: ["script", "r"],
            HelpText = "run a script")]
        internal class Options
        {
            [Value(0, HelpText = "Name of script", Required = true)]
            public string File { get; set; } =string.Empty;
        }
    }

    private void Info(string s)
    {
        _outputConsole.SetForegroundColor(ConsoleColor.Yellow);
        _outputConsole.WriteLine(s);
    }

    public static class QueryCommand
    {
        internal static async Task RunAsync(InteractiveTableExplorer exp, Options o)
        {
            var filename = ToFullPath(o.File, exp._folders.QueryFolder, ".csl");
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
            var filename = ToFullPath(o.File, exp._folders.QueryFolder, ".csl");
            exp.Info($"Saving query to '{filename}'");
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
            HelpText = "save a query to a file so you can use it again")]
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
            var pr = new VirtualConsoleProgressReporter(exp._outputConsole);
            await exp._loader.SaveResult(exp._prevResult, o.File,pr);
        }

        [Verb("save", aliases: ["sv"], HelpText = "save last results to file")]
        internal class Options
        {
            [Value(0, HelpText = "Name of file", Required = true)]
            public string File { get; set; } = string.Empty;


            [Option('b', "bare", HelpText = "Skips header for csv (useful when generating id lists)")]
            public bool SkipHeader { get; set; }
        }
    }

    public static class FileFormatsCommand
    {
        internal static  Task RunAsync(InteractiveTableExplorer exp, Options o)
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
            var text = KustoResultRenderer.RenderToHtml(result);
            File.WriteAllText(fileName, text);
            Process.Start(new ProcessStartInfo { FileName = fileName, UseShellExecute = true });
        }

        [Verb("render", aliases: ["ren"], HelpText = "render last results as html")]
        internal class Options
        {
            [Value(0, HelpText = "Name of file")] public string File { get; set; } = string.Empty;
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

            var pr = new VirtualConsoleProgressReporter(exp._outputConsole);
            var success = await exp._loader.LoadTable(exp._context, o.File, tableName, pr);
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
            foreach (var (k, v) in exp._settings.Enumerate())
            {
                if(!k.Contains(o.Match))
                    continue;
                exp.Info($"{k} -> {v}");
            }
            return Task.CompletedTask;
        }

        [Verb("settings", HelpText = "lists all settings")]
        internal class Options
        {
            [Value(0, HelpText = "match substring", Required = false)]
            public string Match { get; set; } = string.Empty;

            

        }
    }


    private void Warn(string s)
    {
        _outputConsole.SetForegroundColor(ConsoleColor.Red);
        _outputConsole.WriteLine(s);
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
