using AwesomeAssertions;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class TakeAnyTests : TestMethods
{

    [TestMethod] public async Task TakeAnyMultiple()
    {
        // Arrange
        var query = """
                    datatable(x:long, val:string)
                    [
                        0, 'first',
                        1, 'second',
                        2, 'third',
                        3, 'fourth',
                    ]
                    | summarize take_any(val,x)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().ContainAny("first second third fourth".Tokenize());

    }

    [TestMethod]
    public async Task TakeAnySingle()
    {
        // Arrange
        var query = """
                    datatable(x:long, val:string)
                    [
                        0, 'first',
                        1, 'second',
                        2, 'third',
                        3, 'fourth',
                    ]
                    | summarize take_any(val)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().ContainAny("first second third fourth".Tokenize());

    }
    [TestMethod]
    public async Task TakeAnyStar()
    {
        // Arrange
        var query = """
                    datatable(x:long, val:string)
                    [
                        0, 'first',
                        1, 'second',
                        2, 'third',
                        3, 'fourth',
                    ]
                    | summarize take_any(*)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().ContainAny("first second third fourth".Tokenize());

    }
}
