using CommandLine;
using KustoLoco.CopilotSupport;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class CopilotCommand
{
    private static CopilotHelper? _copilot;

    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        var blocks = context.InputProcessor;
        
        if (blocks.IsComplete)
            return;

        if (_copilot == null)
        {
            console.Info("Creating new copilot");
            _copilot = new CopilotHelper(context.Settings, queryContext);
        }

        var query = blocks.ConsumeNextBlock();
        var resp = await _copilot.SendUserRequest(query);
        console.Info(resp.Explanation);
        console.Warn(resp.Code);
        await context.RunInput(resp.Code);
    }

    [Verb("copilot",
        HelpText = "start/run copilot")]
    internal class Options
    {
    }
}
