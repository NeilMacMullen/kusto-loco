using CommandLine;
using NLog;

/// <summary>
///     Interactive data explorer
/// </summary>
internal class CmdRun
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var explorer = await Explorer.Create(options.Data, options.Args,options.Load,
            options.Commands,options.Scripts);
       

        explorer.Close();
    }

    [Verb("run", true, HelpText = "runs scripts and/or commands")]
    public class Options :CommonOptions
    {
      

    }
}
