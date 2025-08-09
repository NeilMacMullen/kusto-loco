using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class MaxOfTests : TestMethods
{



    [TestMethod]
    public async Task MaxOf()
    {
        var res = await LastLineOfResult("print max_of(10,20)");
        res.Should().Be("20");
    }

    [TestMethod] public async Task MaxOfNull()
    {
        var res = await LastLineOfResult("print max_of(10,long(null),20)");
        res.Should().Be("20");
    }
    [TestMethod]
    public async Task MinOfNull()
    {
        var res = await LastLineOfResult("print min_of(10,long(null),20)");
        res.Should().Be("10");
    }

    [TestMethod]
    public async Task MinOf()
    {
        var res = await LastLineOfResult("print min_of(10,20)");
        res.Should().Be("10");
    }


    [TestMethod]
    public async Task MaxOfColumns()
    {
        var query = """
                    datatable (A: long, B: long)
                    [
                        1, 6,
                        8, 1,
                        long(null), 2,
                        1, long(null),
                        long(null), long(null)
                    ]
                    | project max_of(A, B)
                    """;
        var res = await ResultAsString(query);
        res.Should().Be("6,8,2,1,<null>");
    }

    [TestMethod]
    public async Task MinOfColumns()
    {
        var query = """
                    datatable (A: long, B: long)
                    [
                        1, 6,
                        8, 1,
                        long(null), 2,
                        1, long(null),
                        long(null), long(null)
                    ]
                    | project min_of(A, B)
                    """;
        var res = await ResultAsString(query);
        res.Should().Be("1,1,2,1,<null>");
    }

    [TestMethod]
    public async Task MaxOfMany()
    {
        var res = await LastLineOfResult("print max_of(1,2,3,4,100,5,6,7,8)");
        res.Should().Be("100");
    }


}
