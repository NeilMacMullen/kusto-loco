using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
/// Saves last result as named result
/// </summary>
public static class PushCommand
{
    internal static async Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        exp._resultHistory.Save(o.Name);
        await Task.CompletedTask;
    }

    [Verb("push", HelpText =
        @"names and stores the previous result so that it can be used later without rerunning the query
Examples:
  .push result1
  ... run some other queries....
  .save data.csv result1  #save the earlier result to a file
")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
    }
}
