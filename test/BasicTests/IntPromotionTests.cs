using AwesomeAssertions;
using KustoLoco.Core;

namespace BasicTests;

[TestClass]
public class IntPromotionTests : TestMethods
{


    [TestMethod]
    public async Task UnaryMinus()
    {

        var query = """
                    let X = datatable(A:int,B:int)
                    [
                         4,1,
                         5,2,
                    ];
                    X 
                    | extend x=-A
                    | project x
                    | getschema  
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }

    [TestMethod]
    public async Task IntMultInt()
    {

        (await LastLineOfResult("print int(1) *int(1) | getschema"))
        .Should().Contain("long");
    }

    [TestMethod]
    public async Task IntMultIntColumnar()
    {
        var query = """
                    let X = datatable(A:int,B:int)
                    [
                         4,1,
                         5,2,
                    ];
                    X 
                    | extend x=A*B
                    | getschema
                    """;
        (await LastLineOfResult(query))
            .Should().Contain("long");
    }

    [TestMethod]
    public async Task IntAddInt()
    {

        (await LastLineOfResult("print int(1) + int(1) | getschema"))
            .Should().Contain("long");
    }
    [TestMethod]
    public async Task IntMinusInt()
    {

        (await LastLineOfResult("print int(1) - int(1) | getschema"))
            .Should().Contain("long");
    }
    [TestMethod]
    public async Task IntDivideInt()
    {

        (await LastLineOfResult("print int(1) - int(1) | getschema"))
            .Should().Contain("long");
    }

    [TestMethod]
    public async Task IntRemInt()
    {

        (await LastLineOfResult("print int(1) % int(1) | getschema"))
            .Should().Contain("long");
    }

    [TestMethod]
    public async Task UnaryMinusScalar()
    {

        (await LastLineOfResult("let x =int(1) ;print - x| getschema"))
            .Should().Contain("int");
    }

    [TestMethod]
    public async Task SummarizeIntByInt()
    {
        var query = """
                    let data =datatable(c1:int, c2:int)
                    [ 1,2,
                     3,4];
                    data | summarize p=max(c1) by c2
                    | getschema
                    | where
                    """;
        var result = await LastLineOfResult(query);
            result.Should().NotContain("long");
    }
    [TestMethod]
    public async Task SummarizeMaxIntExample()
    {
        var query = """
                    datatable(n:int)[1]
                    | summarize max(n)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("int");
    }

    [TestMethod]
    public async Task MultInts()
    {
        var query = """
                    print int(1)*int(1)
                    | getschema
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }


    [TestMethod]
    public async Task UnaryMinusPrints()
    {
        var query = """
                    print  -int(1)
                    | getschema
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("int");
    }

    [TestMethod]
    public async Task MinShouldPreserveIntType()
    {
        var query = """
                    datatable(Temperature: int)[15, 22, 18, 25, 12]
                    | summarize MinTemp = min(Temperature)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("int");
    }

    [TestMethod]
    public async Task MaxShouldPreserveIntType()
    {
        var query = """
                    datatable(Temperature: int)[15, 22, 18, 25, 12]
                    | summarize MaxTemp = max(Temperature)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("int");
    }

    [TestMethod]
    public async Task CountShouldReturnLong()
    {
        var query = """
                    datatable(Temperature: int)[15, 22, 18, 25, 12]
                    | summarize TotalReadings = count()
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }

    [TestMethod]
    public async Task AvgShouldReturnReal()
    {
        var query = """
                    datatable(Temperature: int)[15, 22, 18, 25, 12]
                    | summarize AvgTemp = avg(Temperature)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("real");
    }

    [TestMethod]
    public async Task GetSchemaDataTypeShouldShowClrType()
    {
        var query = """
                    datatable(Temperature: int)[15, 22, 18, 25, 12]
                    | summarize 
                        TotalReadings = count(),
                        MinTemp = min(Temperature),
                        MaxTemp = max(Temperature),
                        AvgTemp = avg(Temperature)
                    | getschema 
                    | project DataType
                    """;
        var result = await ResultAsLines(query);
        result.Should().Contain("System.Int64");
        result.Should().Contain("System.Int32");
        result.Should().Contain("System.Double");
    }

    [TestMethod]
    public async Task PercentileShouldPromoteIntToLong()
    {
        var query = """
                    datatable(Temperature: int)[15, 22, 18, 25, 12]
                    | summarize Percentile50 = percentile(Temperature, 50)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }

    [TestMethod]
    public async Task AbsShouldPromoteIntToLong()
    {
        var query = """
                    print AbsOfInt = abs(int(-5))
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }

    [TestMethod]
    public async Task ModuloShouldPromoteIntToLong()
    {
        var query = """
                    print ModuloOfInt = int(10) % int(3)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }

    [TestMethod]
    public async Task DivideShouldPromoteIntToLong()
    {
        var query = """
                    print DivideOfInt = int(10) / int(3)
                    | getschema
                    | project ColumnType
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
    }
}
