using CommandLine;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using NLog;
using NotNullStrings;

/// <summary>
///     Interactive data explorer
/// </summary>
internal class CmdRun
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, options.Data);
        var processor = CommandProcessorProvider.GetCommandProcessor();
        var renderer = new SixelRenderingSurface(settings);
        var explorer = new InteractiveTableExplorer(console, settings, processor, renderer, []);
        var block = options.File.IsBlank() ? options.Command : File.ReadAllText(options.File);
        await explorer.RunInput(block);
    }


    public static async Task RunInteractive(IKustoConsole _outputConsole, InteractiveTableExplorer exp)
    {
        exp.Warn("Use '.help' to list commands");

        while (true)
        {
            _outputConsole.ForegroundColor = ConsoleColor.Blue;
            _outputConsole.Write("KQL> ");
            var query = _outputConsole.ReadLine();
            await exp.RunInput(query);
        }
    }

    [Verb("run", true, HelpText = "runs a lokqldx workspace in its entirety")]
    public class Options
    {
        [Option(HelpText = "Default folder to load/save data/results to")]
        public string Data { get; set; } = string.Empty;

        [Option('f', HelpText = "Runs a script at startup", Required = true, SetName = nameof(File))]
        public string File { get; set; } = string.Empty;

        [Option('c', HelpText = "Runs a script provided as a string", Required = true, SetName = nameof(Command))]
        public string Command { get; set; } = string.Empty;
    }
}
