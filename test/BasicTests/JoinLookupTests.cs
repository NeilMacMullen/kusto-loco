using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class JoinLookupTests : TestMethods
{

    [TestMethod]
    public async Task Lookup()
    {
        var query = """
                    let FactTable=datatable(Row:string,Personal:string,Family:string) [
                      "1", "Rowan",   "Murphy",
                      "2", "Ellis",   "Turner",
                      "3", "Ellis",   "Turner",
                      "4", "Maya",  "Robinson",
                      "5", "Quinn",    "Campbell"
                    ];
                    let DimTable=datatable(Personal:string,Family:string,Alias:string) [
                      "Rowan",  "Murphy",   "rowanm",
                      "Ellis",  "Turner", "ellist",
                      "Maya", "Robinson", "mayar",
                      "Quinn",   "Campbell",    "quinnc"
                    ];
                    FactTable
                    | lookup kind=leftouter DimTable on Personal, Family
                    """;
        var result = await ResultAsString(query,Environment.NewLine);
        var expected = """
                       1,Rowan,Murphy,rowanm
                       2,Ellis,Turner,ellist
                       3,Ellis,Turner,ellist
                       4,Maya,Robinson,mayar
                       5,Quinn,Campbell,quinnc
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task JoinInnerUnique()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       b,2,b,10
                       c,4,c,20
                       c,4,c,30
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task JoinInner()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'k',5,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40,
                        'k',50
                    ];
                    X | join kind=inner Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       b,2,b,10
                       b,3,b,10
                       k,5,k,50
                       c,4,c,20
                       c,4,c,30
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task JoinLeftOuter()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=leftouter Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       a,1,,null
                       b,2,b,10
                       b,3,b,10
                       c,4,c,20
                       c,4,c,30
                       """;
        result.Should().Be(expected);
    }

    
    [TestMethod]
    public async Task JoinRightOuter()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=rightouter Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        //Note this is actually a different order than ADX
        var expected = """
                       b,2,b,10
                       b,3,b,10
                       c,4,c,20
                       c,4,c,30
                       ,null,d,40
                       """;
        result.Should().Be(expected);
    }


    [TestMethod]
    public async Task JoinFullOuter()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=fullouter Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        //NOTE this a different order than ADX
        var expected = """
                       a,1,,null
                       b,2,b,10
                       b,3,b,10
                       c,4,c,20
                       c,4,c,30
                       ,null,d,40
                       """;
        result.Should().Be(expected);
    }


    [TestMethod]
    public async Task JoinLeftSemi()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=leftsemi Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       b,2
                       b,3
                       c,4
                       """;
        result.Should().Be(expected);
    }
    [TestMethod]
    public async Task JoinLeftAnti()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=leftanti Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       a,1
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task JoinRightSemi()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=rightsemi Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       b,10
                       c,20
                       c,30
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task JoinRightAnti()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                        'b',3,
                        'c',4
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'b',10,
                        'c',20,
                        'c',30,
                        'd',40
                    ];
                    X | join kind=rightanti Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       d,40
                       """;
        result.Should().Be(expected);
    }
}
