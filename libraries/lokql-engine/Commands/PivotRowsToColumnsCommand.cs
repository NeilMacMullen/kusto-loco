using System.Collections.Specialized;
using CommandLine;
using Kusto.Language.Utils;
using KustoLoco.Core;
using KustoLoco.PluginSupport;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class PivotRowsToColumnsCommand
{
    internal static Task RunAsync(ICommandContext context, Options o)
    {
        var console = context.Console;
        var queryContext = context.QueryContext;
        var history = context.History;
        var result = history.Fetch(o.ResultName);


        var ods = new Dictionary<object, OrderedDictionary>();
        var columns = result.ColumnDefinitions();
        var newColumnNamesIndex = GetColumnIndex(o.ColumnsFrom);
        var valueIndex = GetColumnIndex(o.ValueFrom);
        if (newColumnNamesIndex < 0 || valueIndex < 0)
        {
           console.Warn("Unable to find requested columns");
            return Task.CompletedTask;
        }

        foreach (var row in result.EnumerateRows())
        {
            var key = MakeKey(row);
            var od = ods.TryGetValue(key, out var found)
                ? found
                : new OrderedDictionary();
            ods[key] = od;

            for (var c = 0; c < result.ColumnCount; c++)
                if (c == newColumnNamesIndex)
                {
                    var colHeader = row.GetValue(c)?.ToString().NullToEmpty()!;
                    od[colHeader] = row.GetValue(valueIndex);
                }
                else if (c == valueIndex)
                {
                    //do nothing
                }
                else
                {
                    var colHeader = columns[c].Name;
                    od[colHeader] = row.GetValue(c);
                }
        }

        var builder = TableBuilder.FromOrderedDictionarySet(o.As, ods.Values.ToList());
        queryContext.AddTable(builder);
        console.Info($"Table '{o.As}' now available");
        return Task.CompletedTask;

        int GetColumnIndex(string name)
        {
            return result.ColumnNames().IndexOf(name);
        }

        string MakeKey(object?[] row)
        {
            var key = string.Empty;
            for (var c = 0; c < result.ColumnCount; c++)
            {
                if (c == newColumnNamesIndex || c == valueIndex)
                    continue;
                key += " | " + row[c];
            }

            return key;
        }
    }

    [Verb("pivotRowsToColumns", HelpText = @"pivots values in a column into new column names

The pivotRowsToColumns command is useful for transforming data that is split across
multiple rows into a single row.
For example, this table
   Year | Mode | Distance
   2020 | Sea  | 100
   2020 | Land | 200
   2020 | Air  | 300

can be collapsed into

   Year | Sea | Land | Air
   2020 | 100 | 200  | 300
    
using pivotRowsToColumns --as --columnsFrom Mode --valueFrom Distance

Usage:
 .pivotRowsToColumns [result-name] --as new_table_name --columnsFrom Mode --ValueFrom Distance

")]
    internal class Options
    {
        [Value(0, HelpText = "Name of result to pivot")]
        public string ResultName { get; set; } = string.Empty;

        [Option(Required = true, HelpText = "Name of table into which to project the result")]
        public string As { get; set; } = "pivoted";

        [Option(Required = true, HelpText = "Name column holding data values")]
        public string ValueFrom { get; set; } = string.Empty;

        [Option(Required = true, HelpText = "Name of column containing column names")]
        public string ColumnsFrom { get; set; } = string.Empty;
    }
}
