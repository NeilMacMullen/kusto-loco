using CommandLine;
using KustoLoco.CopilotSupport;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class CopilotCommand
{
    private static CopilotHelper? _copilot;

    internal static async Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        var queryContext = econtext.QueryContext;
        var blocks = econtext.InputProcessor;
        
        if (blocks.IsComplete)
            return;

        if (_copilot == null)
        {
            console.Info("Creating new copilot");
            _copilot = new CopilotHelper(econtext.Settings, queryContext);
        }

        var query = blocks.ConsumeNextBlock();
        var resp = await _copilot.SendUserRequest(query);
        console.Info(resp.Explanation);
        console.Warn(resp.Code);
        await econtext.RunInput(resp.Code);
    }

    [Verb("copilot",
        HelpText = "start/run copilot")]
    internal class Options
    {
    }
}
