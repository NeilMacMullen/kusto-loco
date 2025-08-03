using AwesomeAssertions;

namespace BasicTests;

public class IgnoredTests : TestMethods
{
    
    [TestMethod]
    public async Task Bin_ZeroInterval_ShouldReturnError()
    {
        var query = "print c=bin(10, 0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("null");
    }
    [TestMethod]
    [Ignore("this isn't supported yet")]
    public async Task BetweenStressTest()
    {
        var query = @"
    range i from 1 to 100 step 1
    | extend t = toreal(i), u = toint(i+10)
                                | where i between(t..i)
    ";
        var result = await LastLineOfResult(query);
        result.Should().Be("55");
    }
    [TestMethod]
    [Ignore("It's unclear whether promoting to long is just a bug in ADX")]
    public async Task UnaryMinusColumnar()
    {
        var query = """
                    datatable(n:int)[1]
                    | extend x=-n
                    | project x
                    | getschema
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }
}
