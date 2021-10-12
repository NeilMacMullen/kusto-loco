// See https://aka.ms/new-console-template for more information
using KustoExecutionEngine.Core;

var query = @"
MyTable
| project a, c=a+1, d=a*2
| where a > 1
| summarize by a
";

var playground = new ParserPlayground();

playground.DumpTree(query);


var engine = new StirlingEngine();
engine.Evaluate(query);


Console.WriteLine("Done");
