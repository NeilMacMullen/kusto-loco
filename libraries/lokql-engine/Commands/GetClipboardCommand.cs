using System.Collections.Immutable;
using Clowd.Clipboard;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class GetClipboardCommand
{
    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        if (!OperatingSystem.IsWindows())
            return;
        var console = context.Console;
        var queryContext = context.QueryContext;

        try
        {
            var tableName = o.As;        
       
            var text = await ClipboardAvalonia.GetTextAsync();
            var lines = text.Split(Environment.NewLine).Select(l => new { line = l })
                .ToImmutableArray();
            var table = TableBuilder.CreateFromImmutableData(tableName, lines);
            queryContext.AddTable(table);
            await context.RunInput(tableName);
            console.Info($"Loaded clipboard contents as {tableName}");
        }
        catch (Exception)
        {
            console.Warn($"Unable to load data from clipboard");
        }
    }

    [Verb("clip", HelpText = """
                             Loads lines from the clipboard to a column called 'lines'
                             """
    )]
    internal class Options
    {
        [Value(0, HelpText = "Name of table (defaults to 'clip')")]
        public string As { get; set; } = "clip";

    }
}
