using CommandLine;
using Microsoft.Identity.Client;

namespace Lokql.Engine.Commands;

public static class SaveCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp= econtext.Explorer;
        await exp._loader.SaveResult(exp.GetPreviousResult(), o.File);
    }

    [Verb("save", aliases: ["sv"], HelpText = "save last results to file")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        public string File { get; set; } = string.Empty;
    }
}

public static class PushCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp._resultHistory.Save(o.Name);
       await Task.CompletedTask;
    }

    [Verb("push", aliases: ["sv"], HelpText = "keeps a result in memory ")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
    }
}


public static class PullCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var res = exp._resultHistory.Fetch(o.Name);
        await exp.InjectResult(res);
    }

    [Verb("pop", aliases: ["sv"], HelpText = "keeps a result in memory ")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
    }
}
