using CommandLine;
using NLog;

/// <summary>
///     Interactive data explorer
/// </summary>
internal class CmdExplore
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var explorer = await Explorer.Create(options.Data, options.Args, options.Load,
            options.Commands, ((CommonOptions)options).Scripts);
        await explorer.RunInteractive();
        explorer.Close();
    }


    [Verb("explore", HelpText = "runs scripts and/or commands then stays in interactive mode")]
    public class Options : CommonOptions
    {
    }
}
