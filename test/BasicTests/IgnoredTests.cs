using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class IgnoredTests : TestMethods
{
   
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

    [TestMethod]
    [Ignore("Not yet implemented")]
    public async Task ArrayConcatFunction_Scalar()
    {
        var query = "print result = array_concat(dynamic([1,2]), dynamic([3,4]))";
        var result = await LastLineOfResult(query);
        result.Should().Contain("[1,2,3,4]");
    }

}
