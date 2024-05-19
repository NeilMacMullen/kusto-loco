// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Extensions;
using FluentAssertions;
using KustoLoco.Core.Console;
using Xunit;

namespace KustoExecutionEngine.Core.Tests;

public class NegativeTests
{
    [Fact(Skip = "Not supported yet")]
    public void A()
    {
        // Per docs this should fail, but seems to be accepted.
        // The engine fails to process this at this time, but not for the expected reasons...
        // See: https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/functions/user-defined-functions
        //   > Not supported:
        //   > f is a scalar function that references the tabular expression Table1,
        //   > and is invoked with a reference to the current row context f(Column):

        // Arrange
        var query = @"
let Table1 = datatable(xdate:datetime)[datetime(1970-01-01)];
let Table2 = datatable(Column:long)[1235];
let f = (hours:long) { toscalar(Table1 | summarize min(xdate) - hours*1h) };
Table2 | where Column != 123 | project d = f(Column)
";

        // Act & Assert
        Test(query, string.Empty);
    }

    private static void Test(string query, string expectedOutput)
    {
        var engine = BabyKustoEngine.CreateForTest();
        var result = (TabularResult?)engine.Evaluate(
            Array.Empty<ITableSource>(),
            query);
        Debug.Assert(result != null);
        var stringified = result.Value.DumpToString();

        var canonicalOutput = stringified.Trim().Replace("\r\n", "\n");
        var canonicalExpectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

        canonicalOutput.Should().Be(canonicalExpectedOutput);
    }
}
