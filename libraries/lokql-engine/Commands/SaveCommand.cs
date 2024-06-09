using CommandLine;

namespace Lokql.Engine.Commands;

public static class SaveCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp= econtext.Explorer;
        await exp._loader.SaveResult(exp._prevResult, o.File);
    }

    [Verb("save", aliases: ["sv"], HelpText = "save last results to file")]
    internal class Options
    {
        [Value(0, HelpText = "Name of file", Required = true)]
        public string File { get; set; } = string.Empty;
    }
}
