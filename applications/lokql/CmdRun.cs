using CommandLine;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using NLog;

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
        var loader = new StandardFormatAdaptor(settings, console);
        loader.SetDataPaths(options.Data);
        var processor = CommandProcessorProvider.GetCommandProcessor();
        var explorer = new InteractiveTableExplorer(console, loader, settings, processor);
        var block = File.ReadAllText(options.Run);
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

    [Verb("run", HelpText = "runs a lokqldx workspace in its entirety")]
    public class Options
    {
      
        [Option(HelpText = "Default folder to load/save data/results to")]
        public string Data { get; set; } = string.Empty;



        [Option(HelpText = "Runs a script at startup")]
        public string Run { get; set; } = string.Empty;
    }
}
