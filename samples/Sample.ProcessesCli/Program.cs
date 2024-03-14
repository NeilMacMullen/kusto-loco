// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using KustoLoco.Core;
using NotNullStrings;
using Sample.ProcessesCli;
using Spectre.Console;

if (args.Length == 0)
{
    Console.WriteLine(
        @"

This program demonstrates the use of KQL to query the current process list.  Invoke it like....
processes.exe");
    return;
}

var processes = ProcessReader.GetProcesses();
var query = args.First();

var result = await new KustoQueryContext()
    .AddTableFromImmutableData("processes", processes)
    .RunTabularQueryAsync(query);

if (result.Error.IsNotBlank())
{
    AnsiConsole.MarkupLineInterpolated($"[red]{result.Error}[/]");
    return;
}

//set up Spectre.Console table
var table = new Table();

// Add columns with header names
foreach (var column in result.ColumnDefinitions()) table.AddColumn(column.Name);
// Add rows.  Note that data can be null here  
foreach (var row in result.EnumerateRows())
{
    var rowCells = row.Select(r => r?.ToString() ?? string.Empty).ToArray();
    table.AddRow(rowCells);
}

AnsiConsole.Write(table);