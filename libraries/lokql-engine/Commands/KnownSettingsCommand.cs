using System.Collections.Immutable;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

/// <summary>
///     Lists all the registered settings
/// </summary>
public static class KnownSettingsCommand
{
    internal static async Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var settings = context.Settings;
        var queryContext = context.QueryContext;
        var defs = settings.GetDefinitions()
            .Where(d => d.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(d => d.Name)
            .ToImmutableArray();

        var settingDefsTableName="_knownSettings";
        var table=TableBuilder.CreateFromImmutableData(settingDefsTableName,defs);
        queryContext.AddTable(table);
        await context.RunInput(settingDefsTableName);
        console.Info($"Known settings now available as table {settingDefsTableName}");
    }

    [Verb("knownsettings", HelpText = @"displays all registered setting definitions with descriptions
Shows setting name, description, default value, and type.
Returns results as table '_knownSettings' for further querying.
Examples:
  .knownsettings         # Show all known settings
  .knownsettings csv     # Show CSV-related settings only
  .knownsettings kusto   # Show kusto.* settings")]
    internal class Options
    {
        [Value(0, HelpText = "match substring", Required = false)]
        public string Match { get; set; } = string.Empty;
    }
}
