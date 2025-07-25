using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class ArgMinMaxTests : TestMethods
{

    [TestMethod]
    public async Task ArgMaxStar()
    {
        var query = """
                    datatable(Fruit: string, Color: string, Version: int) [
                        "Apple", "Red", 1,
                        "Apple", "Green", int(null),
                        "Banana", "Yellow", int(null),
                        "Banana", "Green", int(null),
                        "Pear", "Brown", 1,
                        "Pear", "Green", 2,
                    ]
                    | summarize arg_max(Version, *)
                    """;

        var result = await LastLineOfResult(query);
        result.Should().Be("2,Pear,Green");
    }

    [TestMethod]
    public async Task ArgMinStar()
    {
        var query = """
                    datatable(Fruit: string, Color: string, Version: long) [
                        "Apple", "Red", -1,
                        "Apple", "Green", int(null),
                        "Banana", "Yellow", int(null),
                        "Banana", "Green", int(null),
                        "Pear", "Brown", 1,
                        "Pear", "Green", 2,
                    ]
                    | summarize arg_min(Version, *)
                    """;

        var result = await LastLineOfResult(query);
        result.Should().Be("-1,Apple,Red");
    }
   

    [TestMethod]
    public async Task ArgMaxMultiple()
    {
        var query = """
                    datatable(Fruit: string, Color: string, Version: int,Price: long) [
                        "Apple", "Red", 1,100,
                        "Pear", "Brown", 1,200,
                        "Pear", "Green", 2,300,
                    ]
                    | summarize arg_max(Price,Fruit,Color)
                    """;

        var result = await LastLineOfResult(query);
        result.Should().Be("300,Pear,Green");
    }



    [TestMethod]
    public async Task ArgMaxNamed()
    {
        var query = """
                    datatable(Fruit: string, Color: string, Version: long) [
                        "Apple", "Red", 1,
                        "Apple", "Green", 5,
                        "Banana", "Yellow", 6,
                        "Banana", "Green", 7,
                        "Pear", "Brown", 1,
                        "Pear", "Green", 2,
                    ]
                    | summarize arg_max(Version, Color) by Fruit
                    | order by Version asc
                    """;

        var result = await LastLineOfResult(query);
        result.Should().Be("Banana,7,Green");
    }
}
