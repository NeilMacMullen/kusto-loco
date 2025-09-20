using AwesomeAssertions;

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
        var query = "print -1 *5";
        var result = await LastLineOfResult(query);
        result.Should().Be("-5");
    }


    [TestMethod]
    public async Task NegativeExpression()
    {
        var query = "print -(1+5)";
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
        var query = "print geo_geohash_to_central_point('sunny')";

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


    [TestMethod]
    public async Task ArrayReverse2()
    {
        var query = """
                    print arr=dynamic([1, 2, 3, 4]) 
                    | project Result=array_reverse(arr)
                    """;

        var result = await LastLineOfResult(query);
        result.Should().Be("""
                           [
                             4,
                             3,
                             2,
                             1
                           ]
                           """);
    }


    [TestMethod]
    public async Task ArrayIif1()
    {
        // query1
        var query = """
                    print condition=dynamic([true,false,true]), if_true=dynamic([1,2,3]), if_false=dynamic([4,5,6]) 
                    | extend res= array_iif(condition, if_true, if_false)
                    | extend res_str = strcat_array(res, ",")
                    | project res_str
                    """;
        var expected = "1,5,3";
        var res = await LastLineOfResult(query);

        res.Should().Be(expected);
    }

    [TestMethod]
    public async Task ArrayIif2()
    {
        // query2
        var query2 = """
                     print condition=dynamic([1,0,50]), if_true = dynamic(["yes"]), if_false=dynamic(["no"]) 
                     | extend res= array_iif(condition, if_true, if_false)
                     | extend res_str = strcat_array(res, ",")
                     | project res_str
                     """;
        var expected2 = "yes,no,yes";
        var res2 = await LastLineOfResult(query2);
        res2.Should().Be(expected2);
    }

    [TestMethod]
    public async Task ArrayIif3()
    {
        // //query 3
        var query3 = """
                     print condition=dynamic(["some string value", "datetime(01-01-2022)", null]), if_true=dynamic(["1"]), if_false=dynamic(["0"])
                     | extend res= array_iif(condition, if_true, if_false)
                     | extend res_str = strcat_array(res, ",")
                     | project res_str
                     """;
        var expected3 = "null,null,null";
        var res3 = await LastLineOfResult(query3);
        res3.Should().Be(expected3);
    }

    [TestMethod]
    public async Task ArrayIif4()
    {
        // query 4
        var query4 = """
                     print condition=dynamic([true,true,true]), if_true=dynamic([1,2]), if_false=dynamic([3,4]) 
                     | extend res= array_iif(condition, if_true, if_false)
                     | extend res_str = strcat_array(res, ",")
                     | project res_str
                     """;
        var expected4 = "1,2,\"null\"";
        var res4 = await LastLineOfResult(query4);
        res4.Should().Be(expected4);
    }

    [TestMethod]
    public async Task ArrayIff()
    {
        // query 4
        var query4 = """
                     print condition=dynamic([true,true,true]), if_true=dynamic([1,2]), if_false=dynamic([3,4]) 
                     | extend res= array_iff(condition, if_true, if_false)
                     | extend res_str = strcat_array(res, ",")
                     | project res_str
                     """;
        var expected4 = "1,2,\"null\"";
        var res4 = await LastLineOfResult(query4);
        res4.Should().Be(expected4);
    }
}
