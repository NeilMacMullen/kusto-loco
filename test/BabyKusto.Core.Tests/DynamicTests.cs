// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
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
        public void ParseJson_Scalar_HappyCases(string jsonString, string expectedValue)
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
        public void ParseJson_Scalar_UnhappyCases(string jsonString, string expectedValue)
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

        [Fact]
        public void ParseJson_Columnar()
        {
            // Arrange
            string query = @"
datatable(json:string) [
    ""null"",
    ""{}"",
    ""[]"",
    ""[1, 2]"",
    ""[{\""a\"": 1}]"",
    ""{\""a\"": \""b\""}"",
    """",
    "" "",
    ""invalid"",
    ""{'a': 'b'}""
]
| project a=parse_json(json)
";

            string expected = @"
a:dynamic
------------------
(null)
{}
[]
[1,2]
[{""a"":1}]
{""a"":""b""}
(null)
"" ""
""invalid""
""{'a': 'b'}""
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void DynamicLiteral()
        {
            // Arrange
            string query = @"
print a=dynamic([1,2,3]), b=dynamic({""a"":1})
";

            string expected = @"
a:dynamic; b:dynamic
------------------
[1,2,3]; {""a"":1}
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void DynamicLiteral_LooseJson_Explodes()
        {
            // Arrange
            string query = @"
print a=dynamic({'a':1})
";

            // Act & Assert
            var func = () => Test(query, expectedOutput: "whatever");

            // This test exercises a BabyKusto limitation, at least confirm we are consistently failing
            func.Should().ThrowExactly<InvalidOperationException>().Which.Message.Should().Be("A literal dynamic expression was provided that is valid in Kusto but doesn't conform to the stricter JSON parser used by BabyKusto. Please rewrite the literal expression to be properly formatted JSON (invalid input: \"{'a':1}\")");
        }

        [Fact]
        public void PathMemberAccess_Scalar()
        {
            // Arrange
            string query = @"
let obj1=dynamic({""a"":1});
let obj2=dynamic({""a"":{""b"":{""c"":123}}});
print a=obj1.a, b=obj1.b, c=obj2.a.b.c
";

            string expected = @"
a:dynamic; b:dynamic; c:dynamic
------------------
1; (null); 123
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void PathMemberAccess_Columnar()
        {
            // Arrange
            string query = @"
datatable(obj:dynamic) [
    dynamic({""a"":1}),
    dynamic({""a"":""c"", ""b"":2}),
    dynamic({""a"":{""b"":{""c"":123}}}),
]
| project a=obj.a, b=obj.b, c=obj.a.b.c
";

            string expected = @"
a:dynamic; b:dynamic; c:dynamic
------------------
1; (null); (null)
""c""; 2; (null)
{""b"":{""c"":123}}; (null); 123
";

            // Act & Assert
            Test(query, expected);
        }

        [Fact]
        public void ElementMemberAccess()
        {
            // Arrange
            string query = @"
print a=dynamic({""a"":1})[""a""]
";

            string expected = @"
a:dynamic
------------------
1
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