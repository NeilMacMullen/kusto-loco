using CommandLine;

namespace Lokql.Engine.Commands;

public static class ListSettingsCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var settings = exp.Settings.Enumerate()
            .Where(s => s.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(s => s.Name);

        var str = Tabulator.Tabulate(settings, "Name|Value", s => s.Name, s => s.Value);
        exp.Info(str);
        return Task.CompletedTask;
    }

    [Verb("settings", HelpText = "lists all settings")]
    internal class Options
    {
        [Value(0, HelpText = "match substring", Required = false)]
        public string Match { get; set; } = string.Empty;
    }
}
