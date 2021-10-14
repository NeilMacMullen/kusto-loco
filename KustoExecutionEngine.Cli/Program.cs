// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using KustoExecutionEngine.Core;

var query = @"
let c=100.0;
MyTable
| project frac=CounterValue/c, AppMachine, CounterName
| summarize avg(frac) by CounterName
";

var engine = new StirlingEngine();
var myTable = new InMemoryTabularSourceV2(
    new TableSchema(
        new List<ColumnDefinition>()
        {
            new ColumnDefinition("AppMachine",   KustoValueKind.String),
            new ColumnDefinition("CounterName",  KustoValueKind.String),
            new ColumnDefinition("CounterValue", KustoValueKind.Real),
        }),
        new[]
        {
            new Column(new object?[] { "vm0", "vm0", "vm1", "vm1", "vm2" }),
            new Column(new object?[] { "cpu", "mem", "cpu", "mem", "cpu" }),
            new Column(new object?[] {  50.0,  30.0,  20.0,  5.0,   100.0 }),
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
