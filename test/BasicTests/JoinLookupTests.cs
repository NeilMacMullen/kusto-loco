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
                       a,1,,<null>
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
                       ,<null>,d,40
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
                       a,1,,<null>
                       b,2,b,10
                       b,3,b,10
                       c,4,c,20
                       c,4,c,30
                       ,<null>,d,40
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



    [TestMethod]
    public async Task LookupLeftOuterWithFailedLookups()
    {
        var query = """
                    let X = datatable(Key:string, Value1:long)
                    [
                        'a',1,
                        'b',2,
                    ];
                    let Y = datatable(Key:string, Value2:long)
                    [
                        'a',10,
                    ];
                    X | lookup Y on Key
                    """;
        var result = await ResultAsString(query, Environment.NewLine);
        var expected = """
                       a,1,10
                       b,2,<null>
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task CheckThatRightTableColumnIndicesAreCorrectlyUsed()
    {
        var query = """
                    datatable(Site:string, 
                    SiteId:guid,
                    SiteCode:string)
                    [
                        "Place 1", 
                        guid(0803cff9-b16b-4d5f-a7b9-05f03c8d38a2), 
                        "75256"
                    ]
                    | join kind = rightanti datatable(Line:string, SiteCode:string)
                    [
                        "Place2", "61930"
                    ]  on SiteCode
                    """;
        var result = await LastLineOfResult(query);
        var expected = """
                       Place2,61930
                       """;
        result.Should().Be(expected);
    }

    //left key is at index 2, right key is at index 0
    private const string DifferentIndexTables = """
                    let L = datatable(A:string, B:long, Key:string)
                    [
                        'p',1,'a',
                        'q',2,'b',
                        'r',3,'c'
                    ];
                    let R = datatable(Key:string, C:long)
                    [
                        'b',10,
                        'c',20,
                        'd',30
                    ];

                    """;

    private async Task CheckDifferentIndices(string operation, string expected)
    {
        var result = await ResultAsString(DifferentIndexTables + operation, Environment.NewLine);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task RightSemiJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=rightsemi R on Key", """
                                                                           b,10
                                                                           c,20
                                                                           """);

    [TestMethod]
    public async Task RightAntiJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=rightanti R on Key", """
                                                                           d,30
                                                                           """);

    [TestMethod]
    public async Task LeftSemiJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=leftsemi R on Key", """
                                                                          q,2,b
                                                                          r,3,c
                                                                          """);

    [TestMethod]
    public async Task LeftAntiJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=leftanti R on Key", """
                                                                          p,1,a
                                                                          """);

    [TestMethod]
    public async Task InnerJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=inner R on Key", """
                                                                      q,2,b,b,10
                                                                      r,3,c,c,20
                                                                      """);

    [TestMethod]
    public async Task LeftOuterJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=leftouter R on Key", """
                                                                          p,1,a,,<null>
                                                                          q,2,b,b,10
                                                                          r,3,c,c,20
                                                                          """);

    [TestMethod]
    public async Task RightOuterJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=rightouter R on Key", """
                                                                           q,2,b,b,10
                                                                           r,3,c,c,20
                                                                           ,<null>,,d,30
                                                                           """);

    [TestMethod]
    public async Task FullOuterJoinWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | join kind=fullouter R on Key", """
                                                                           p,1,a,,<null>
                                                                           q,2,b,b,10
                                                                           r,3,c,c,20
                                                                           ,<null>,,d,30
                                                                           """);

    [TestMethod]
    public async Task LookupWithDifferentKeyIndices()
        => await CheckDifferentIndices("L | lookup R on Key", """
                                                                p,1,a,<null>
                                                                q,2,b,10
                                                                r,3,c,20
                                                                """);

    private const string MultiKeyTables = """
                    let L = datatable(A:string, K2:string, K1:string)
                    [
                        'p','x','a',
                        'q','y','b'
                    ];
                    let R = datatable(K1:string, V:long, K2:string)
                    [
                        'a',1,'x',
                        'b',2,'x',
                        'b',3,'y',
                        'c',4,'y'
                    ];

                    """;

    [TestMethod]
    public async Task RightSemiJoinMultipleKeysDifferentIndices()
    {
        var result = await ResultAsString(MultiKeyTables + "L | join kind=rightsemi R on K1, K2",
            Environment.NewLine);
        var expected = """
                       a,1,x
                       b,3,y
                       """;
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task RightAntiJoinMultipleKeysDifferentIndices()
    {
        var result = await ResultAsString(MultiKeyTables + "L | join kind=rightanti R on K1, K2",
            Environment.NewLine);
        var expected = """
                       b,2,x
                       c,4,y
                       """;
        result.Should().Be(expected);
    }

    private const string WideRightTables = """
                    let L = datatable(A:string, Key:string)
                    [
                        'p','a',
                        'q','b'
                    ];
                    let R = datatable(P:long, Q:long, S:long, Key:string)
                    [
                        1,2,3,'b',
                        4,5,6,'z'
                    ];

                    """;

    [TestMethod]
    public async Task RightSemiJoinWithRightKeyIndexBeyondLeftColumnCount()
    {
        var result = await ResultAsString(WideRightTables + "L | join kind=rightsemi R on Key",
            Environment.NewLine);
        result.Should().Be("1,2,3,b");
    }

    [TestMethod]
    public async Task RightAntiJoinWithRightKeyIndexBeyondLeftColumnCount()
    {
        var result = await ResultAsString(WideRightTables + "L | join kind=rightanti R on Key",
            Environment.NewLine);
        result.Should().Be("4,5,6,z");
    }
}
