using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
/// Retrieves a named result
/// </summary>
public static class PullCommand
{
    internal static async Task RunAsync(ICommandContext econtext, Options o)
    {
        var res = econtext.History.Fetch(o.Name);
        await econtext.InjectResult(res);
    }

    [Verb("pull",  HelpText = "retrieves a stored result and displays it")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
    }
}
