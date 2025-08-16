﻿using System.Collections.Specialized;
using CommandLine;
using Kusto.Language.Utils;
using KustoLoco.Core;
using KustoLoco.PluginSupport;

namespace Lokql.Engine.Commands;

public static class PivotColumnsToRowsCommand
{
    internal static Task RunAsync(ICommandContext econtext, Options o)
    {
        var console = econtext.Console;
        var queryContext = econtext.QueryContext;

        var result = econtext.History.Fetch(o.ResultName);
        var specialColumns = o.Columns.Select(GetColumnIndex).ToArray();
        var boringColumns = Enumerable.Range(0, result.ColumnCount).Except(specialColumns)
            .ToArray();
        if (specialColumns.Any(i => i < 0))
        {
            console.Warn("Some column names not found");
            return Task.CompletedTask;
        }
        var ods = new List<OrderedDictionary>();
        var columns = result.ColumnDefinitions();
        foreach (var row in result.EnumerateRows())
        {
            foreach(var i in specialColumns)
            {
                var od = new OrderedDictionary
                {
                    [o.ColumnNamesInto] = columns[i].Name,
                    [o.ValuesInto] = row[i]
                };
                foreach (var k in boringColumns)
                {
                    od[columns[k].Name] = row[k];
                }
                ods.Add(od);
            }
        }

        var builder = TableBuilder.FromOrderedDictionarySet(o.As, ods);
        queryContext.AddTable(builder);
        console.Info($"Table '{o.As}' now available");
        return Task.CompletedTask;
        int GetColumnIndex(string name) => result.ColumnNames().IndexOf(name);
    }

    [Verb("pivotColumnsToRows", HelpText = @"pivots columns into rows

The pivotColumnsToRows command is useful for transforming data that has multiple columns
that would be better expressed as a single column with a type value.
For example, this table
   Year | Sea | Land | Air
   2020 | 100 | 200  | 300
    
might be better expressed as
   Year | Mode | Distance
   2020 | Sea  | 100
   2020 | Land | 200
   2020 | Air  | 300

.pivotColumnsToRows --Columns  Sea Land Air --ValuesInto Distance --ColumnNamesInto Mode

")]
    internal class Options
    {
        [Value(0, HelpText = "Name of result")]
        public string ResultName { get; set; } = string.Empty;

        [Option(HelpText = "Name of table into which to project the result")]
        public string As { get; set; } = "pivoted";

        [Option(Required=true,HelpText = "Columns to move into rows")]
        public IEnumerable<string> Columns { get; set; } = [];

       
        [Option(Required=true,HelpText = "Name of the new column to hold values")]
        public string ValuesInto { get; set; } = "Value";

        [Option(Required=true,HelpText = "Name of the column to hold the previous column names")]
        public string ColumnNamesInto { get; set; } = "Value";
    }
}
