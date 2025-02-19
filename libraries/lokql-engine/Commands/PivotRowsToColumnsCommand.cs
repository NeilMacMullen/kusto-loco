using System.Collections.Specialized;
using CommandLine;
using Kusto.Language.Utils;
using KustoLoco.Core;
using NotNullStrings;

namespace Lokql.Engine.Commands;

public static class PivotRowsToColumnsCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var result = exp._resultHistory.Fetch(o.ResultName);
        int GetColumnIndex(string name) => result.ColumnNames().IndexOf(name);


        var ods = new Dictionary<object, OrderedDictionary>();
        var columns = result.ColumnDefinitions();
        var newColumnNamesIndex = GetColumnIndex(o.ColumnsFrom);
        var valueIndex = GetColumnIndex(o.ValueFrom);
        if (newColumnNamesIndex < 0 || valueIndex < 0 )
        {
            exp.Warn("Unable to find requested columns");
            return Task.CompletedTask;
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

        foreach (var row in result.EnumerateRows())
        {
            var key = MakeKey(row);
            var od = ods.TryGetValue(key, out var found)
                         ? found
                         : new OrderedDictionary();
            ods[key] = od;

            for (var c = 0; c < result.ColumnCount; c++)
            {
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
        }

        var builder = TableBuilder.FromOrderedDictionarySet(o.As, ods.Values.ToList());
        exp.GetCurrentContext().AddTable(builder);
        return Task.CompletedTask;
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

        [Option(Required=true,HelpText = "Name of table into which to project the result")]
        public string As { get; set; } = string.Empty;

        [Option(Required = true, HelpText = "Name column holding data values")]
        public string ValueFrom { get; set; } = string.Empty;

        [Option(Required = true, HelpText = "Name of column containing column names")]
        public string ColumnsFrom { get; set; } = string.Empty;
    }
}
