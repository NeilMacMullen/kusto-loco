// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;

Console.WriteLine(@"/----------------------------------------------------------------\");
Console.WriteLine(@"| Welcome to BabyKusto.ProcessQuerier. You can write KQL queries |");
Console.WriteLine(@"| and explore the live list of processes on your machine.        |");
Console.WriteLine(@"\----------------------------------------------------------------/");
Console.WriteLine();

ShowDemos();

while (true)
{
    try
    {
        PrintCaret();
        var query = Console.ReadLine();
        if (query == null || query == "exit")
        {
            return;
        }

        ExecuteReplQuery(query);
    }
    catch (Exception ex)
    {
        var lastColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Error:");
        Console.WriteLine(ex);
        Console.ForegroundColor = lastColor;
    }

    Console.WriteLine();
}

static void ShowDemos()
{
    var demos = new[]
    {
        (Title: "Example: counting the total number of processes:", Query: @"Processes | count"),
        (Title: "Example: Find the process using the most memory:", Query: @"Processes | project name, memMB=workingSet/1024/1024 | order by memMB desc | take 1")
    };

    foreach (var demo in demos)
    {
        ShowDemo(demo.Title, demo.Query);
    }
}

static void ShowDemo(string title, string query)
{
    var lastColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(title);
    Console.ForegroundColor = lastColor;
    PrintCaret();
    Console.WriteLine(query);
    ExecuteReplQuery(query);
}

static void ExecuteReplQuery(string query)
{
    var processesTable = GetProcessesTable();
    var engine = new BabyKustoEngine();
    engine.AddGlobalTable("Processes", processesTable);
    var result = engine.Evaluate(query, dumpIRTree: true); // Set dumpIRTree = true to see the internal tree representation

    Console.WriteLine();

    if (result is TabularResult tabularResult)
    {
        tabularResult.Value.Dump(Console.Out);
    }

    Console.WriteLine();
}

static InMemoryTableSource GetProcessesTable()
{
    var names = new ColumnBuilder<string?>(ScalarTypes.String);
    var numThreads = new ColumnBuilder<int?>(ScalarTypes.Int);
    var workingSets = new ColumnBuilder<long?>(ScalarTypes.Long);

    foreach (var p in Process.GetProcesses())
    {
        string processName;
        int processThreads;
        long workingSet;
        try
        {
            processName = p.ProcessName;
            processThreads = p.Threads.Count;
            workingSet = p.WorkingSet64;
        }
        catch { continue; }

        names.Add(processName);
        numThreads.Add(processThreads);
        workingSets.Add(workingSet);
    }

    var myTable = new InMemoryTableSource(
        TableSymbol.From("(name:string, numThreads:int, workingSet:long)"),
        new Column[] { names.ToColumn(), numThreads.ToColumn(), workingSets.ToColumn() });
    return myTable;
}

static void PrintCaret()
{
    var lastColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("> ");
    Console.ForegroundColor = lastColor;
}