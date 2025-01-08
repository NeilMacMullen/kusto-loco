using System.Collections.Specialized;
using CommandLine;
using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public static class PivotCommand
{
    internal static Task RunAsync(CommandProcessorContext econtext, Options o)
    {
        var exp = econtext.Explorer;
        var result = exp._resultHistory.Fetch(o.ResultName);
        var ods = new List<OrderedDictionary>();
        var columns = result.ColumnDefinitions();
        foreach (var row in result.EnumerateRows())
            for (var i = o.Keep; i < row.Length; i++)
            {
                var od = new OrderedDictionary();

                for (var k = 0; k < o.Keep; k++)
                    od[columns[k].Name] = row[k];

                od[o.Key] = columns[i].Name;
                od[o.Value] = row[i];
                ods.Add(od);
            }

        var builder = TableBuilder.FromOrderedDictionarySet(o.As, ods);
        exp.GetCurrentContext().AddTable(builder);
        return Task.CompletedTask;
    }

    [Verb("pivot", HelpText = @"pivots columns into rows

The pivot command is useful for transforming data that has multiple columns
that would be better expressed as a single column with a type value.
For example, this table
   Year | Sea | Land | Air
   2020 | 100 | 200  | 300
    
might be better expressed as
   Year | Mode | Value
   2020 | Sea  | 100
   2020 | Land | 200
   2020 | Air  | 300

Usage:
 .pivot [result-name] --as new_table_name --keep 1 --key Mode --value Value

The --keep option specifies the number of columns to keep before pivoting.
")]
    internal class Options
    {
        [Value(0, HelpText = "Name of result")]
        public string ResultName { get; set; } = string.Empty;

        [Option(HelpText = "Name of table into which to project the result")]
        public string As { get; set; } = string.Empty;

        [Option(HelpText = "Number of columns before first pivoted column")]
        public int Keep { get; set; } = 1;

        [Option(HelpText = "Name of new discriminator column")]
        public string Key { get; set; } = "Key";

        [Option(HelpText = "Name of new data column")]
        public string Value { get; set; } = "Value";
    }
}
