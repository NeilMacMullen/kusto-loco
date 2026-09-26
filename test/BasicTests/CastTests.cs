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


    [TestMethod]
    public async Task BetweenRealTest()
    {
        // Arrange
        var query = """
                    datatable(A:real)[1.345]
                    | where A between (1..2)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("1.345");
        var schema = await LastLineOfResult($"{query} | getschema");
        schema.Should().Contain("real");

    }

    [TestMethod]
    public async Task BetweenDecimalTest()
    {
        // Arrange
        var query = """
                    datatable(A:decimal)[1.345]
                    | where A between (1..2)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("1.345");

        var schema = await LastLineOfResult($"{query} | getschema");
        schema.Should().Contain("decimal");

    }


    [TestMethod]
    public async Task BetweenLongTest()
    {
        // Arrange
        var query = """
                    datatable(A:long)[3]
                    | where A between (1.2..4.5)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("3");

        var schema = await LastLineOfResult($"{query} | getschema");
        schema.Should().Contain("long");

    }
}
