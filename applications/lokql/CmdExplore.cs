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
        var explorer = new Explorer(options.Data, options.Args);
        await explorer.RunInteractive();
        explorer.Close();
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

        [Option("args", HelpText = "Passes arguments to the script as arg0, arg1 etc")]
        public IEnumerable<string> Args { get; set; } = [];
    }
}
