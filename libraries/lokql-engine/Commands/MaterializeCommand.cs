using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class MaterializeCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        var history = context.History;
        queryContext.MaterializeResultAsTable(history.Fetch(o.ResultName), o.As);
        console.Info($"Table {NameEscaper.EscapeIfNecessary(o.As)} now available");
        return Task.CompletedTask;
    }

    [Verb("materialize", aliases: ["materialise", "mat"],
        HelpText = @"saves a result back into the context as a table
Examples:
  .materialize t                # materializes the last result as a table called 't'
  .materialize t storedResult   # materializes a stored result as a table called 't'
  ")]
    internal class Options
    {
        [Value(0, Required = true, HelpText = "Name of table")]
        public string As { get; set; } = string.Empty;
        [Value(1, HelpText = "Name of table")]
        public string ResultName { get; set; } = string.Empty;
    }
}
