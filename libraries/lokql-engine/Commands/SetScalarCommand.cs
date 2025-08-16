using CommandLine;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Saves last result as named result
/// </summary>
public static class SetScalarCommand
{
    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var settings = context.Settings;
        var history = context.History;
        var result =history.Fetch(o.Result);
        var text = (result.RowCount > 0) ? result.Get(0, 0)?.ToString() ?? "<null>" : "no data";
        settings.Set(o.Name, text);
        console.Info($"Set {o.Name} to {text}");
        await Task.CompletedTask;
    }

    [Verb("setscalar", HelpText =
        @"Sets the value in cell 0,0 of the named result as a value in the settings table
Examples:
print format_datetime(now(),'yyyy-MM-dd')
.setscalar file
.save $(file).csv earlier_result
")]
    internal class Options
    {
        [Value(0, HelpText = "Name", Required = true)]
        public string Name { get; set; } = string.Empty;
        [Value(1, HelpText = "Result")]
        public string Result { get; set; } = string.Empty;
    }
}
