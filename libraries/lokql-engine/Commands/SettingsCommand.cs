using System.Collections.Immutable;
using CommandLine;
using DocumentFormat.OpenXml.Wordprocessing;
using KustoLoco.Core;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

/// <summary>
/// show all current settings and values
/// </summary>
public static class SettingsCommand
{
    internal static Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        var queryContext = econtext.QueryContext;
        var settings = econtext.Settings;
        var matches = settings.Enumerate()
            .Where(s => s.Name.Contains(o.Match, StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(s => s.Name);
        var settingsTableName = "_settings";
        var table = 
            TableBuilder.CreateFromImmutableData(settingsTableName, matches
                .Select(s => new { Name = s.Name,Value=s.Value })
                .OrderBy(s=>s.Name)
                .ToImmutableArray());

        queryContext.AddTable(table);
        econtext.RunInput(settingsTableName);
        console.Info($"Settings available as table '{settingsTableName}'");
        return Task.CompletedTask;
    }

    [Verb("settings", HelpText = "shows all entries in the settings table")]
    internal class Options
    {
        [Value(0, HelpText = "match substring", Required = false)]
        public string Match { get; set; } = string.Empty;
    }
}
