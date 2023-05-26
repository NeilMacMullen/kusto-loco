// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using BabyKusto.Core;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using FluentAssertions;
using Xunit;

namespace KustoExecutionEngine.Core.Tests
{
    public class DynamicTests
    {
        [Theory]
        [InlineData("null", "(null)")]
        [InlineData("{}", "{}")]
        [InlineData("[]", "[]")]
        [InlineData("[1, 2]", "[1,2]")]
        [InlineData("[{\"a\": 1}]", "[{\"a\":1}]")]
        [InlineData("{\"a\": \"b\"}", "{\"a\":\"b\"}")]
        public void ParseJson_HappyCases(string jsonString, string expectedValue)
        {
            // Arrange
            string query = $@"
print a=parse_json(""{EscapeKustoString(jsonString)}"")
";

            string expected = $@"
a:dynamic
------------------
{expectedValue}
";

            // Act & Assert
            Test(query, expected);
        }

        [Theory]
        [InlineData("", "(null)")]
        [InlineData(" ", "\" \"")]
        [InlineData("invalid", "\"invalid\"")]
        [InlineData("{\'a\': \'b\'}", "\"{'a': 'b'}\"")] // single-quotes are invalid json
        public void ParseJson_UnhappyCases(string jsonString, string expectedValue)
        {
            // Arrange
            string query = $@"
print a=parse_json(""{EscapeKustoString(jsonString)}"")
";

            string expected = $@"
a:dynamic
------------------
{expectedValue}
";

            // Act & Assert
            Test(query, expected);
        }

        private static string EscapeKustoString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\"", "\\\"")
                .Replace("\'", "\\\'");
        }

        private static void Test(string query, string expectedOutput)
        {
            var engine = new BabyKustoEngine();
            var result = (TabularResult?)engine.Evaluate(query);
            Debug.Assert(result != null);
            var stringified = result.Value.DumpToString();

            var canonicalOutput = stringified.Trim().Replace("\r\n", "\n");
            var canonicalExpectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

            canonicalOutput.Should().Be(canonicalExpectedOutput);
        }
    }
}