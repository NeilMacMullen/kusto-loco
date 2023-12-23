using System.Diagnostics;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Extensions;
using FluentAssertions;
using Xunit;

namespace BabyKusto.Core.Tests;

public class ScalarFunctionsTests
{
    [Fact]
    public void TestLevenshtein()
    {
        var query = """
                let Table1 = datatable(value1:string,value2:string)['test123','test124'];
                Table1 | where levenshtein(value1, value2) == 1
                """;

        var expected = @"
value1:string; value2:string
------------------
test123; test124
";

        Test(query, expected);
    }

    [Fact]
    public void TestSimilarityFunction()
    {
        var query = """
                let Table1 = datatable(value1:string,value2:string)['test123','test124'];
                Table1 | where stringSimilarity(value1, value2) < 90
                """;
        var expected = @"
value1:string; value2:string
------------------
test123; test124
";
        Test(query, expected);
    }

    [Fact]
    public void TestSimilarityFunctionWithBlankInput()
    {
        var query = """
                let Table1 = datatable(value1:string,value2:string)['test123',''];
                Table1 | where stringSimilarity(value1, value2) < 90
                """;
        var expected = @"
value1:string; value2:string
------------------
test123; 
";
        Test(query, expected);
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
