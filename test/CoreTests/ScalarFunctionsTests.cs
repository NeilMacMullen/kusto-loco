using System;
using System.Diagnostics;
using AwesomeAssertions;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Extensions;
using Xunit;

namespace KustoLoco.Core.Tests;

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
                    print s= toint((string_similarity('test123', 'test124') *100))
                    """;
        var expected = @"
s:int
------------------
85
";
        Test(query, expected);
    }

    [Fact]
    public void TestSimilarityFunctionWithBlankInput()
    {
        var query = """
                    let Table1 = datatable(value1:string,value2:string)['test123',''];
                    Table1 | where string_similarity(value1, value2) < 0.9
                    """;
        var expected = @"
value1:string; value2:string
------------------
test123; 
";
        Test(query, expected);
    }

    [Fact]
    public void EnsureTwoBlankStringsAre100PercentSimilar()
    {
        var query = """
                    print s=string_similarity('', '')
                    """;
        var expected = @"
s:real
------------------
1 
";
        Test(query, expected);
    }

    [Fact]
    public void TestDateTimeToIsoFunctionWithBlankInput()
    {
        var query = """
                    let Table1 = datatable(value1:string,value2:datetime)['test123', datetime(2023-02-03T14:30:00.00)];
                    Table1 | project iso = datetime_to_iso(value2)
                    """;
        var expected = @"
iso:string
------------------
2023-02-03T14:30:00.0000000 
";
        // Act and Assert
        Test(query, expected);
    }

    [Fact]
    public void TestBase64Encode()
    {
        var query = """
                    print base64=base64_encode_tostring("This string will be base64 encoded")
                    """;
        var expected = @"
base64:string
------------------
VGhpcyBzdHJpbmcgd2lsbCBiZSBiYXNlNjQgZW5jb2RlZA==
";
        Test(query, expected);
    }

    [Fact]
    public void TestBase64Decode()
    {
        var query = """
                    print text=base64_decode_tostring("VGhpcyBzdHJpbmcgd2lsbCBiZSBiYXNlNjQgZW5jb2RlZA==")
                    """;
        var expected = @"
text:string
------------------
This string will be base64 encoded
";
        Test(query, expected);
    }

    private static void Test(string query, string expectedOutput)
    {
        var engine = BabyKustoEngine.CreateForTest();
        var result = (TabularResult?)engine.Evaluate(
            [],
            query);
        Debug.Assert(result != null);
        var stringified = result.Value.DumpToString();

        var canonicalOutput = stringified.Trim().Replace("\r\n", "\n");
        var canonicalExpectedOutput = expectedOutput.Trim().Replace("\r\n", "\n");

        canonicalOutput.Should().Be(canonicalExpectedOutput);
    }
}
