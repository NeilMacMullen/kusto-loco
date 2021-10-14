// See https://aka.ms/new-console-template for more information

using System;
using System.Collections.Generic;
using KustoExecutionEngine.Core;
using KustoExecutionEngine.Core.Extensions;

var query = @"
let c=100.0;
MyTable
| where AppMachine != 'vm1'
| project frac=CounterValue/c, AppMachine, CounterName
| summarize avg(frac) by CounterName
| project CounterName, avgRoundedPercent=tolong(avg_frac*100)
";

var engine = new StirlingEngine();
var myTable = new InMemoryTableSource(
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

Console.WriteLine("----------------------------------------------------------------------------");
Console.WriteLine("Welcome to StirlingEngine, the little self-contained Kusto execution engine!");
Console.WriteLine("----------------------------------------------------------------------------");
Console.WriteLine();

Console.WriteLine("MyTable:");
myTable.Dump(Console.Out, indent: 4);

Console.WriteLine();
Console.WriteLine("Query:");
Console.WriteLine(query);

Console.WriteLine();
Console.WriteLine("Result:");

if (result is ITableSource tableResult)
{
    tableResult.Dump(Console.Out, indent: 4);
}
