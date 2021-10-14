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
| summarize count() by Column1
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
    foreach (var columnDef in tabularResult.Schema.ColumnDefinitions)
    {
        Console.Write(columnDef.ColumnName);
        Console.Write("; ");
    }
    Console.WriteLine("------------------");

    foreach (var chunk in tabularResult.GetData())
    {
        for (int i = 0; i < chunk.RowCount; i++)
        {
            var row = chunk.GetRow(i);
            for (int j = 0; j < row.Values.Length; j++)
            {
                Console.Write(row.Values[j]);
                Console.Write("; ");
            }
            Console.WriteLine();
        }
    }
    Console.WriteLine();
}

Console.WriteLine("Done");
