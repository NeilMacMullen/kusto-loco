using System.Collections.Specialized;
using CommandLine;
using KustoLoco.Core;

namespace Lokql.Engine.Commands;

public static class PivotCommand
{
    internal static void Run(InteractiveTableExplorer exp, Options o)
    {
        var result = exp._prevResult;
        var ods = new List<OrderedDictionary>();
        var columns = result.ColumnDefinitions();
        foreach (var row in result.EnumerateRows())
            for (var i = o.Keep; i < row.Length; i++)
            {
                var od = new OrderedDictionary();

                for (var k = 0; k < o.Keep; k++)
                    od[columns[k].Name] = row[k];

                od["Column"] = columns[i].Name;
                od["Data"] = row[i];
                ods.Add(od);
            }

        var builder = TableBuilder.FromOrderedDictionarySet(o.As, ods);
        exp.GetCurrentContext().AddTable(builder);
    }

    [Verb("pivot", HelpText = "pivots columns into rows")]
    internal class Options
    {
        [Value(0, Required = true, HelpText = "Name of table")]
        public string As { get; set; } = string.Empty;

        [Option(HelpText = "Columns before first pivoted column")]
        public int Keep { get; set; } = 1;
    }
}
