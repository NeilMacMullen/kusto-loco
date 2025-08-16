using System.Collections.Immutable;
using CommandLine;
using KustoLoco.Core;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class ListTablesCommand
{
    internal static Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        var queryContext = econtext.QueryContext;
        var tableNames = queryContext.TableNames
            .Select(t => new { Table = NameEscaper.EscapeIfNecessary(t) })
            .ToImmutableArray();
        const string tableName = "_tables";
        var table = TableBuilder.CreateFromImmutableData(tableName, tableNames);
        queryContext.AddTable(table);
        econtext.RunInput(tableName);
        console.Info($"Table names are now available as table {tableName}");
        return Task.CompletedTask;
    }

    [Verb("listtables", aliases: ["ls", "tables"],
        HelpText = "Lists all available tables")]
    internal class Options
    {
    }
}
