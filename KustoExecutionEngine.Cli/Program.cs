// See https://aka.ms/new-console-template for more information
using KustoExecutionEngine.Core;

var query = @"
let c=10.0;
MyTable
| project plus10=a+c, mulBy2=a+a
";

var playground = new ParserPlayground();

playground.DumpTree(query);


var engine = new StirlingEngine();
var result = engine.Evaluate(query);
if (result is ITabularSource tabularResult)
{
    IRow? row;
    while ((row = tabularResult.GetNextRow()) != null)
    {
        Console.WriteLine(string.Join(", ", row.Select(r => $"{r.Key}={r.Value}")));
    }
}

Console.WriteLine("Done");
