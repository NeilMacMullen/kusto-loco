// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using FluentAssertions;
using Xunit;

namespace KustoExecutionEngine.Core.Tests
{
    public class KustoDocsTests
    {
        [Fact]
        public void A()
        {
            // From: https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/functions/user-defined-functions

            // Arrange
            string query = @"
// Supported:
// f is a scalar function that doesn't reference any tabular expression
let Table1 = datatable(xdate:datetime)[datetime(1970-01-01)];
let Table2 = datatable(Column:long)[1235];
let f = (hours:long) { datetime(2022-02-19 19:00) + hours*1h };
Table2 | where Column != 123 | project d = f(10)
";

            string expected = @"
d:datetime
------------------
2022-02-20 05:00:00Z
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact(Skip = "This is broken for now")]
        public void B()
        {
            // From: https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/functions/user-defined-functions

            // Arrange
            string query = @"
// Supported:
// f is a scalar function that references the tabular expression Table1,
// but is invoked with no reference to the current row context f(10):
let Table1 = datatable(xdate:datetime)[datetime(1970-01-01)];
let Table2 = datatable(Column:long)[1235];
let f = (hours:long) { toscalar(Table1 | summarize min(xdate) - hours*1h) };
Table2 | where Column != 123 | project d = f(10)
";

            string expected = @"
d:datetime
------------------
2022-02-19 19:00
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact(Skip = "Default or named arguments are not supported")]
        public void C()
        {
            // From: https://docs.microsoft.com/en-us/azure/data-explorer/kusto/query/functions/user-defined-functions

            // Arrange
            string query = @"
let f = (a:long, b:string = 'b.default', c:long = 0) {
  strcat(a, '-', b, '-', c)
};
union
  (print x=f(c=7, a=12)), // '12-b.default-7'
  (print x=f(12, c=7))    // '12-b.default-7'
";

            string expected = @"
d
------------------
12-b.default-7
12-b.default-7
";

            // Act & Assert
            Test(query, expected);
        }

        private static void Test(string query, string expectedOutput)
        {
            var engine = new BabyKustoEngine();
            var result = (TabularResult)engine.Evaluate(query);
            var stringified = result.Value.DumpToString();

            var canonicalOutput = stringified.Trim().Replace("\r\n", "\n");
            var canonicalExpectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

            canonicalOutput.Should().Be(canonicalExpectedOutput);
        }
    }
}