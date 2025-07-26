using AwesomeAssertions;

namespace BasicTests;

public class IgnoredTests : TestMethods
{
    [Ignore]
    [TestMethod]
    public async Task Bin_ZeroInterval_ShouldReturnError()
    {
        var query = "print c=bin(10, 0)";
        var result = await LastLineOfResult(query);
        result.Should().Be("null");
    }
}
