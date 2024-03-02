using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Text;
using BabyKusto.Core.Evaluation;
using CommandLine;
using Extensions;
using KustoSupport;
using NLog;
using static KustoSupport.KustoFormatter;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local setters are
// required on Options properties by CommandLineParser library

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

#pragma warning disable CS8604 // Possible null reference argument.

public class ReportExplorer
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly KustoQueryContext _context = new();

    private readonly FolderContext _folders;
    private readonly IKustoQueryContextTableLoader _loader;
    private readonly IConsole _outputConsole;
    private readonly StringBuilder commandBuffer = new();

    private DisplayOptions _currentDisplayOptions = new(10);
    private KustoQueryResult _prevResult;

    public ReportExplorer(IConsole outputConsole, FolderContext folders, IKustoQueryContextTableLoader loader)
    {
        _outputConsole = outputConsole;
        _loader = loader;
        _context.AddLazyTableLoader(_loader);
        _folders = folders;
    }

    public ReportExplorer(IConsole outputConsole, FolderContext folders) : this(outputConsole, folders,
        new StandardFormatAdaptor(folders.OutputFolder))
    {
    }

    private KustoQueryContext GetCurrentContext() => _context;

    private void ShowResultsToConsole(KustoQueryResult result, int start, int maxToDisplay)
    {
        _outputConsole.SetForegroundColor(ConsoleColor.Green);


        var prefs = new DisplayPreferences(_outputConsole.WindowWidth, start, maxToDisplay);
        _outputConsole.WriteLine(Tabulate(result, prefs));

        if (maxToDisplay < result.Height)
        {
            _outputConsole.SetForegroundColor(ConsoleColor.Red);
            _outputConsole.WriteLine(
                $"Display was truncated to first {maxToDisplay} of {result.Height}.  Use '.display --max' to change this behaviour");
        }
    }

    private void DisplayResults(KustoQueryResult result, bool autoRender)
    {
        if (result.Error.IsNotBlank())
            ShowError(result.Error);
        else
        {
            _outputConsole.SetForegroundColor(ConsoleColor.Green);
            if (result.Height == 0)
            {
                _outputConsole.WriteLine("No results");
            }
            else
            {
                var max = _currentDisplayOptions.MaxToDisplay;
                ShowResultsToConsole(result, 0, max);
                //auto render

                if (autoRender && result.Visualization != VisualizationState.Empty)
                {
                    RenderCommand.Run(this, new RenderCommand.Options());
                }
            }

            _outputConsole.SetForegroundColor(ConsoleColor.Yellow);
            _outputConsole.WriteLine($"Query took {result.QueryDuration}ms");
        }
    }

    public async Task RunInteractive(string initialScript)
    {
        _outputConsole.WriteLine("Use '.help' to list commands");

        if (initialScript.IsNotBlank())
        {
            await RunInput($".run {initialScript}", true);
        }

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
            commandBuffer.Append(query.Substring(0, query.Length - 1) + " ");
            return KustoQueryResult.Empty;
        }

        if (query.EndsWith("|"))
        {
            commandBuffer.Append(query);
            return KustoQueryResult.Empty;
        }

        commandBuffer.Append(query);
        query = commandBuffer.ToString().Trim();
        commandBuffer.Clear();
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


            var result = await GetCurrentContext().RunTabularQueryAsync(query);
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
        string OutputFolder,
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
                    typeof(ShowCommand.Options)
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
            ;
    }


    public static class RunScriptCommand
    {
        internal static async Task RunAsync(ReportExplorer exp, Options o)
        {
            var filename = ToFullPath(o.File, exp._folders.ScriptFolder, ".dfr");
            Logger.Info($"Loading '{filename}'..");
            var lines = await File.ReadAllLinesAsync(filename);
            foreach (var line in lines)
            {
                await exp.ExecuteAsync(line);
            }
        }

        [Verb("run", aliases: ["script", "r"],
            HelpText = "run a script")]
        internal class Options
        {
            [Value(0, HelpText = "Name of script", Required = true)]
            public string File { get; set; }
        }
    }

    public static class QueryCommand
    {
        internal static async Task RunAsync(ReportExplorer exp, Options o)
        {
            var filename = ToFullPath(o.File, exp._folders.QueryFolder, ".csl");
            Logger.Info($"Fetching query '{filename}'");
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
        internal static async Task RunAsync(ReportExplorer exp, Options o)
        {
            var filename = ToFullPath(o.File, exp._folders.QueryFolder, ".csl");
            Logger.Info($"Saving query to '{filename}'");
            var q = exp._prevResult.Query;
            if (!o.NoSplit)
                q = q
                        .Tokenise("|")
                        .JoinString($"{Environment.NewLine}| ")
                        .Tokenise(";")
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
            public string File { get; set; }

            [Option('c', HelpText = "Adds a comment")]
            public string Comment { get; set; } = string.Empty;

            [Option(HelpText = "Prevents query being split into multiple lines at '|' and ';'")]
            public bool NoSplit { get; set; }
        }
    }

    public static class SaveCommand
    {
        internal static async Task RunAsync(ReportExplorer exp, Options o)
        {
            await exp._loader.SaveResult(exp._prevResult, o.File);
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

    public static class ShowCommand
    {
        internal static async Task RunAsync(ReportExplorer exp, Options o)
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
        internal static void Run(ReportExplorer exp, Options o)
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
        internal static void Run(ReportExplorer exp, Options o)
        {
            exp.GetCurrentContext().MaterializeResultAsTable(exp._prevResult, o.As);
            Logger.Info($"Table '{o.As}' now available");
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
        internal static void Run(ReportExplorer exp, Options o)
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
        internal static async Task RunAsync(ReportExplorer exp, Options o)
        {
            var tableName = o.As.OrWhenBlank(Path.GetFileNameWithoutExtension(o.File));
            await exp._loader.LoadTable(exp._context, o.File, tableName);
        }

        [Verb("load", aliases: ["ld"],
            HelpText = "loads a data file")]
        internal class Options
        {
            [Value(0, HelpText = "Name of file", Required = true)]
            public string File { get; set; } = string.Empty;

            [Value(1, HelpText = "Name of table (defaults to name of file)")]
            public string As { get; set; } = string.Empty;
        }
    }


    public static class SynTableCommand
    {
        internal static void Run(ReportExplorer exp, Options o)
        {
            Logger.Info($"Creating synonym for table {o.CurrentName} as {o.As} ...");

            exp.GetCurrentContext().ShareTable(o.CurrentName, o.As);
        }

        [Verb("synomym", aliases: ["syn", "alias"], HelpText = "provides a synonym for a table")]
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
        internal static void Run(ReportExplorer exp, Options o)
        {
            if (o.Max > 0)
                exp._currentDisplayOptions = exp._currentDisplayOptions with { MaxToDisplay = o.Max };
            Logger.Info($"Set: {exp._currentDisplayOptions}");
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
        internal static void Run(ReportExplorer exp, Options o)
        {
            Logger.Info("Exiting...");
            Environment.Exit(0);
        }

        [Verb("exit", aliases: ["quit"], HelpText = "Exit application")]
        internal class Options;
    }

    #endregion
}