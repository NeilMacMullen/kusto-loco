using CommandLine;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
/// show all current settings and values
/// </summary>
public static class SettingsCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var settings = exp.Settings.Enumerate()
            .Where(s => s.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(s => s.Name);

        var str = o.Names ?
                settings.Select(s=>s.Name).JoinAsLines()
            :Tabulator.Tabulate(settings, "Name|Value", s => s.Name, s => s.Value);
        exp.Info(str);
        return Task.CompletedTask;
    }

    [Verb("settings", HelpText = "shows all entries in the settings table")]
    internal class Options
    {
        [Value(0, HelpText = "match substring", Required = false)]
        public string Match { get; set; } = string.Empty;
        [Option(HelpText = "Only show names, not values", Required = false)]
        public bool Names { get; set; } 
    }
}
