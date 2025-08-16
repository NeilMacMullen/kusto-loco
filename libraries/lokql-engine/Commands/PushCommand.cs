using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Saves last result as named result
/// </summary>
public static class PushCommand
{
    internal static async Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        econtext.History.Store(o.Name);
        console.Info($"result stored as '{o.Name}'");
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
