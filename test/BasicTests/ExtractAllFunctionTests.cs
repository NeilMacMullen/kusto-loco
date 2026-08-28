using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class ExtractAllFunctionTests : TestMethods
{
    [TestMethod]
    public async Task ExtractAll_NoGroups_FullMatches()
    {
        var query = "print tostring(extract_all(@'\\d+', 'a1b22c333'))";
        var result = await LastLineOfResult(query);
        result.Should().Be("[\"1\",\"22\",\"333\"]");
    }

    [TestMethod]
    public async Task ExtractAll_OneGroup()
    {
        var query = "print tostring(extract_all(@'(\\d)\\d', '12 34 56'))";
        var result = await LastLineOfResult(query);
        result.Should().Be("[\"1\",\"3\",\"5\"]");
    }

    [TestMethod]
    public async Task ExtractAll_NoMatch_EmptyArray()
    {
        var query = "print tostring(extract_all(@'\\d+', 'abc'))";
        var result = await LastLineOfResult(query);
        result.Should().Be("[]");
    }
}
