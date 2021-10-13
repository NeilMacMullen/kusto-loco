// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Linq;
using KustoExecutionEngine.Core;

var query = @"
let c=10.0;
MyTable
| project a+c,b,d=tolong(a+c)
| summarize count() by b
";


var engine = new StirlingEngine();
engine.AddGlobalTable(
    "MyTable",
    new[] { "a", "b" },
    new[]
    {
        new Row(
            new[]
            {
                new KeyValuePair<string, object?>("a", 1.0),
                new KeyValuePair<string, object?>("b", 2.0),
            }),
        new Row(
            new[]
            {
                new KeyValuePair<string, object?>("a", 1.5),
                new KeyValuePair<string, object?>("b", 2.0),
            }),
    });
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
