using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class ArgMinMaxTests : TestMethods
{
    [TestMethod]
    public async Task Small()
    {
        var query = """
                    datatable(A: int, B:int) [
                    1,2,
                    3,3,
                    ]
                    | summarize arg_max(A) by B
                    """;
        var result = await ResultAsString(query);
        result.Should().Be("2,1,3,3");
    }

    public async Task SmallStar()
    {
        var query = """
                    datatable(A: int, B:int) [
                    1,2,
                    3,3,
                    ]
                    | summarize arg_max(A) by B
                    """;
        var result = await ResultAsLines(query);
        result.Should().Be("""
                           2,1
                           3,3
                           """);
    }

    [TestMethod]
    public async Task SmallA()
    {
        var query = """
                    datatable(A: int, B:int) [
                    1,2,
                    3,3,
                    ]
                    | summarize arg_max(A)
                    """;
        var result = await ResultAsString(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task Small2()
    {
        var query = """
                    datatable(A: int, B:int,C:int) [
                    1,2,7,
                    3,3,7,
                    ]
                    | summarize arg_max(A,C) by B
                    """;
        var result = await ResultAsLines(query);
        result.Should().Be("""
                           2,1,7
                           3,3,7
                           """);
    }

    [TestMethod]
    public async Task MultiColumns()
    {
        var query = """
                    datatable(A: int, B:int,C:int,D:int) [
                    1,2,7,10,
                    1,10,5,5,
                    3,3,7,10,
                    ]
                    | summarize arg_max(B,C,D) by A
                    """;
        var result = await ResultAsLines(query);
        result.Should().Be("""
                           1,10,5,5
                           3,3,7,10
                           """);
    }

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

    [TestMethod]
    public async Task Arg_max()
    {
        var query = "print x=1,y=2 | summarize arg_max(x,*) by y";
        var result = await LastLineOfResult(query);
        result.Should().Be("2,1");
    }
}
