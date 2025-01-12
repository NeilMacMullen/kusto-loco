using CommandLine;

namespace Lokql.Engine.Commands;

using KustoLoco.CopilotSupport;

public static class CopilotCommand
{
    private static CopilotHelper? _copilot;
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var blocks = econtext.Sequence;
       
        if (blocks.Complete)
            return;

        if (_copilot == null)
        {
            exp.Info("Creating new copilot");
            _copilot = new CopilotHelper(exp.Settings,exp.GetCurrentContext());
        }
            var query = blocks.Next();
        var resp = await _copilot.SendUserRequest(query);
        exp.Info(resp.Code);
        await exp.RunInput(resp.Code);
       
    }

    [Verb("copilot", 
        HelpText = @"start/run copilot")]
    internal class Options
    {
      
    }
}
