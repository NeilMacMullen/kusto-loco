using CommandLine;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using Lokql.Engine;
using NLog;
using NotNullStrings;

/// <summary>
///     Provides some summary info on a report
/// </summary>
internal class CmdExplore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
      
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        var loader = new StandardFormatAdaptor(settings,new ConsoleProgressReporter());
        loader.SetDataPaths(options.Data);
        var explorer = new InteractiveTableExplorer(console,  loader, settings);

        await explorer.RunInteractive(options.Run);
    }

    [Verb("explore")]
    public class Options
    {
        [Option(HelpText = "Folder containing '.dfr' scripts")]
        public string Scripts { get; set; } = string.Empty;

        [Option(HelpText = "Folder containing '.csl' scripts")]
        public string Queries { get; set; } = string.Empty;

        [Option(HelpText = "Default folder to load/save data/results to")]
        public string Data { get; set; } = string.Empty;

        [Option(HelpText = "Default folder for Scripts,Queries, Data if those aren't specified")]
        public string WorkIn { get; set; } = string.Empty;


        [Option(HelpText = "Runs a script at startup")]
        public string Run { get; set; } = string.Empty;
    }
}
