using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class RepeatTests : TestMethods
{
    [TestMethod]
    public async Task Repeat()
    {
        var query = """
                     print repeat(1, 3)
                    """;
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,1,1]");
    }
}
