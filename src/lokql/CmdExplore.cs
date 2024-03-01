using CommandLine;
using Extensions;
using NLog;

/// <summary>
///     Provides some summary info on a report
/// </summary>
internal class CmdExplore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var context =
            new ReportExplorer.FolderContext(
                options.WorkIn.OrWhenBlank(options.Results),
                options.WorkIn.OrWhenBlank(options.Scripts),
                options.WorkIn.OrWhenBlank(options.Queries));
        var console = new SystemConsole();
        var explorer = new ReportExplorer(console,context);

        await explorer.RunInteractive(options.Run);
    }

    [Verb("explore")]
    public class Options
    {

        [Option(HelpText = "Folder containing '.dfr' scripts")]
        public string Scripts { get; set; } = string.Empty;

        [Option(HelpText = "Folder containing '.csl' scripts")]
        public string Queries { get; set; } = string.Empty;

        [Option(HelpText = "Default folder to save results to")]
        public string Results { get; set; } = string.Empty;

        [Option(HelpText = "Default folder for Scripts,Queries, Results if those aren't specified")]
        public string WorkIn { get; set; } = string.Empty;


        [Option(HelpText = "Runs a script at startup")]
        public string Run { get; set; } = string.Empty;
    }
}