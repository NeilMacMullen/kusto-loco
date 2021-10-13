// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using System.Linq;
using KustoExecutionEngine.Core;
using KustoExecutionEngine.Core.DataSource;
using ColumnDefinition = KustoExecutionEngine.Core.DataSource.ColumnDefinition;

var query = @"
let c=10.0;
MyTable
| project Column1=1,(a+c)
";

var engine = new StirlingEngine();
var tableSchema = new TableSchema(new List<ColumnDefinition>()
{
    new ColumnDefinition("a", KustoValueKind.Real),
    new ColumnDefinition("b", KustoValueKind.Real),
});
var tableChunk = new TableChunk(tableSchema, new Column[] { new Column(2), new Column(2) });
tableChunk.Columns[0][0] = 1.0;
tableChunk.Columns[0][1] = 1.5;
tableChunk.Columns[1][0] = 2.0;
tableChunk.Columns[1][1] = 2.5;
engine.AddGlobalTable("MyTable", tableSchema, tableChunk);
var result = engine.Evaluate(query);
if (result is ITabularSourceV2 tabularResult)
{
    foreach (var resultChunk in tabularResult)
    {
        for (int i = 0; i < resultChunk.Columns[0].Size; i++)
        {
            Console.WriteLine(string.Join(", ", resultChunk.GetRow(i).Select(r => $"{r.Key}={r.Value}")));
        }
    }
}

Console.WriteLine("Done");
