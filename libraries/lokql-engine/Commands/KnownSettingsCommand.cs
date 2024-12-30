using CommandLine;

namespace Lokql.Engine.Commands;

/// <summary>
/// Lists all the registered settings
/// </summary>
public static class KnownSettingsCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var defs = exp.Settings.GetDefinitions()
            .Where(d => d.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(d => d.Name);

        var str = Tabulator.Tabulate(defs, "Name|Description|Default", d => d.Name, d => d.Description,
            d => d.DefaultValue);
        exp.Info(str);
        return Task.CompletedTask;
    }

    [Verb("knownsettings", HelpText = "lists all setting definitions known to the engine")]
    internal class Options
    {
        [Value(0, HelpText = "match substring", Required = false)]
        public string Match { get; set; } = string.Empty;
    }
}
