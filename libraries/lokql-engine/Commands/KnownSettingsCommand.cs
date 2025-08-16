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

    [Verb("knownsettings", HelpText = @"lists all setting definitions known to the engine
Examples:
  .knownsettings         #show all known settings
  .knownsettings csv     #show known settings that related to csv processing")]
    internal class Options
    {
        [Value(0, HelpText = "match substring", Required = false)]
        public string Match { get; set; } = string.Empty;
    }
}
