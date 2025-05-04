using FluentAssertions;

namespace BasicTests;

[TestClass]
public class DynamicTests : TestMethods
{
    [TestMethod]
    public async Task MemberAccess()
    {
        var query = @"print o=dynamic({""a"":123}) | project z=o.a";
        var result = await LastLineOfResult(query);
        result.Should().Be("123");
    }

    [TestMethod]
    public async Task NestedMemberAccess()
    {
        var query = @"print o=dynamic({""a"":{""b"":456}}) | project z=o.a.b";
        var result = await LastLineOfResult(query);
        result.Should().Be("456");
    }

    [TestMethod]
    public async Task ArrayAccess()
    {
        var query = @"print o=dynamic({""a"":[1,2]}) | project z=o.a[0]";
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }

    [TestMethod]
    public async Task NegativeNumber()
    {
        var query = @"print -1 *5";
        var result = await LastLineOfResult(query);
        result.Should().Be("-5");
    }


    [TestMethod]
    public async Task NegativeExpression()
    {
        var query = @"print -(1+5)";
        var result = await LastLineOfResult(query);
        result.Should().Be("-6");
    }

    [TestMethod]
    public async Task ArrayAccessWithNegativeIndex()
    {
        var query = @"print o=dynamic({""a"":[1,2]}) | project z=o.a[-1]";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task DynamicArray()
    {
        var query = @"print geo_geohash_to_central_point('sunny')";

        var result = await LastLineOfResult(query);
        result.Should().Contain("coordinates");
    }

    [TestMethod]
    public async Task JsonArrayCorrectlyTyped()
    {
        var query = """
                    let Config = datatable(Key:string, Value:string) [
                      "RequiredFolders", "folderA,folderB",
                      "RequiredFolders2", "folderA,folderC"
                      ];
                    Config        
                    | where Key == "RequiredFolders"
                    | extend SplitValues = split(Value, ",")
                    | extend ConfigValid = SplitValues contains "folderA"
                    | project ConfigValid
                      
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("True");
    }


    [TestMethod]
    public async Task ArrayLength()
    {
        var query = """print array_length(dynamic([1, 2, 3, "four"]))""";

        var result = await LastLineOfResult(query);
        result.Should().Be("4");
    }

    [TestMethod]
    public async Task ArrayReverse()
    {
        var query = """
                    print arr=dynamic(["this", "is", "an", "example"]) 
                    | project Result=array_reverse(arr)
                    | project A=Result[0]
                    
                    """;

        var result = await LastLineOfResult(query);
        result.Should().Be("example");
    }


}
