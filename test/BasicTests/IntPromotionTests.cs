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
                    | getschema  
                    """;
        var result = await LastLineOfResult(query);

        //TODO = actually ADX turns this into a long
        //but let x=int(1);print -x | getschema gives "int"! 
        result.Should().Contain("int");
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
    public async Task SummarizeLongByInt()
    {
        var query = """
                    let data =datatable(c1:int, c2:int)
                    [ 1,2,
                     3,4];
                    data | summarize p=max(c1) by c2
                    | getschema
                    """;
        var result = await LastLineOfResult(query);
            result.Should().Contain("long");
    }
    [TestMethod]
    public async Task SummarizeLongByIntExample()
    {
        var query = """
                    datatable(n:int)[1]
                    | summarize max(n)
                    | getschema
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("long");
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

   
}
