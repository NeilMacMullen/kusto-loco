using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class CastTests : TestMethods
{

    [TestMethod]
    public async Task Cast()
    {
        // Arrange
        var query = @"print a=tolong('')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("null");

    }
}
