using System.Diagnostics.CodeAnalysis;
using System.Net.Quic;
using System.Text;
using CommandLine;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using NLog;

/// <summary>
///     Interactive data explorer
/// </summary>
internal class CmdExplore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, options.Data);

        var processor = CommandProcessorProvider.GetCommandProcessor();
        var renderer = new SixelRenderingSurface(settings);
        var explorer = new InteractiveTableExplorer(console, settings, processor, renderer);
        await RunInteractive(console, explorer);
    }


    public static async Task RunInteractive(IKustoConsole _outputConsole, InteractiveTableExplorer exp)
    {
        exp.Warn("Use '.help' to list commands");

        var isContinuation = false;
        var query = new StringBuilder();
        while (true)
        {
            _outputConsole.ForegroundColor = ConsoleColor.Blue;
            var prompt = isContinuation ? "   > " : "KQL> ";
            _outputConsole.Write(prompt);
            var queryPart = _outputConsole.ReadLine().Trim();
            var isSlashContinuation = queryPart.EndsWith(@"\");
            isContinuation = isSlashContinuation
                             || queryPart.EndsWith("|");
            if (isSlashContinuation)
                queryPart = queryPart.Substring(0, queryPart.Length - 1);
            query.Append(queryPart);
            if (!isContinuation)
            {
                await exp.RunInput(query.ToString());
                query.Clear();
            }
        }
    }

    [Verb("explore", HelpText = "explore data source(s)")]
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
