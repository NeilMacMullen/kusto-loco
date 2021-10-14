// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
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
var myTable = new InMemoryTabularSourceV2(
    new TableSchema(
        new List<ColumnDefinition>()
        {
            new ColumnDefinition("a", KustoValueKind.Real),
            new ColumnDefinition("b", KustoValueKind.Real),
        }),
        new[]
        {
            new Column(new object?[] { 1.0, 2.0 }),
            new Column(new object?[] { 1.5, 2.5 }),
        });
engine.AddGlobalTable("MyTable", myTable);
var result = engine.Evaluate(query);
if (result is ITabularSourceV2 tabularResult)
{
    foreach (var columnDef in tabularResult.Schema.ColumnDefinitions)
    {
        Console.Write(columnDef.ColumnName);
        Console.Write("; ");
    }
    Console.WriteLine();
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
