// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using Kusto.Language.Symbols;

Console.WriteLine("-----------------------------------------------------------------------");
Console.WriteLine("Welcome to BabyKusto, the little self-contained Kusto execution engine!");
Console.WriteLine("-----------------------------------------------------------------------");
Console.WriteLine();

var query = @"
let c=100.0;
MyTable
| where AppMachine == 'vm1'
| project frac=CounterValue/c, AppMachine, CounterName
| summarize avg(frac) by CounterName
";

var myTable = new InMemoryTableSource(
    TableSymbol.From("(AppMachine:string, CounterName:string, CounterValue:real)").WithName("MyTable"),
    new Column[]
    {
        Column.Create(ScalarTypes.String, new [] { "vm0", "vm0", "vm1", "vm1", "vm2" }),
        Column.Create(ScalarTypes.String, new [] { "cpu", "mem", "cpu", "mem", "cpu" }),
        Column.Create(ScalarTypes.Real, new double?[] { 50.0, 30.0, 20.0, 5.0, 100.0 }),
    });

Console.WriteLine("MyTable:");
myTable.Dump(Console.Out, indent: 4);
Console.WriteLine();

Console.WriteLine("Query:");
Console.WriteLine(query.Trim());
Console.WriteLine();

var engine = new BabyKustoEngine();
engine.AddGlobalTable("MyTable", myTable);
var result = engine.Evaluate(query, dumpIRTree: true);

Console.WriteLine();
Console.WriteLine("Result:");

if (result is TabularResult tabularResult)
{
    tabularResult.Value.Dump(Console.Out, indent: 4);
}
