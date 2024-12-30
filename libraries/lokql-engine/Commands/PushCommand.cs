using CommandLine;

namespace Lokql.Engine.Commands;

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
