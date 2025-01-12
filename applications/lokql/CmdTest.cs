using CommandLine;
using KustoLoco.CopilotSupport;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using Lokql.Engine.Commands;
using NLog;

internal class CmdTest
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public static async Task RunAsync(Options options)
    {

        var template = OrchestratorMethods.GetTemplate();
        Console.WriteLine(template);
        
        await Task.CompletedTask;
    }


    [Verb("test", HelpText = "")]
    public class Options
    {
      
    }
}
