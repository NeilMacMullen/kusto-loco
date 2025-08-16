using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class SynTableCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        console.Info($"Creating synonym for table {o.CurrentName} as {NameEscaper.EscapeIfNecessary(o.As)} ...");
        queryContext.ShareTable(o.CurrentName, o.As);
        return Task.CompletedTask;
    }

    [Verb("synonym", aliases: ["syn", "alias"], HelpText =
        @"Allows a table to be referred to using an alternate name
Examples:
   .synonym ['awkward table name']  t   #allows the awkward table to be referred to as 't'")]
    internal class Options
    {
        [Value(0, HelpText = "table name", Required = true)]
        public string CurrentName { get; set; } = string.Empty;

        [Value(1, HelpText = "Synonym", Required = true)]
        public string As { get; set; } = string.Empty;
    }
}
