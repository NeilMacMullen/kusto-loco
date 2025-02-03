using CommandLine;
using KustoLoco.CopilotSupport;
using NLog;

internal class CmdTest
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {
        var template = SystemPromptCreator.GetTemplate();
        Console.WriteLine(template);

        await Task.CompletedTask;
    }


    [Verb("test", HelpText = "")]
    public class Options
    {
    }
}
