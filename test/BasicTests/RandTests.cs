using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class RandTests : TestMethods
{
    [TestMethod]
    public async Task Rand()
    {
        //difficult to test randomness but ensure we got
        //5 different values and they were all <1
        var query = """
                    range i from 1 to 5 step 1 
                    | extend r =rand()
                    | where r >=0 and r <1
                    | summarize by r | count 
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("5");
    }

    [TestMethod]
    public async Task RandAvg()
    {
        //ensure we didn't get any fractional values
        var query = """
                    range i from 1 to 1000 step 1 
                    | extend r=rand(100)
                    |summarize avg(r)

                    """;
        var result = await LastLineOfResult(query);
        //check good distribution
        double.Parse(result).Should().BeInRange(25, 75);
    }

    [TestMethod]
    public async Task RandOne()
    {
        //ensure we didn't get any fractional values
        var query = """
                    range i from 1 to 1000 step 1 
                    | extend r=rand()
                    | summarize avg(r)

                    """;
        var result = await LastLineOfResult(query);
        //check good distribution
        double.Parse(result).Should().BeInRange(0.45, 0.55);
    }

    [TestMethod]
    public async Task RandProvidesInt()
    {
        //ensure we didn't get any fractional values
        var query = """
                    range i from 1 to 1000 step 1 
                    | extend r=rand(100)
                    | where toint(r) != r
                    | count
                    """;
        var result = await LastLineOfResult(query);
        //check good distribution
        result.Should().Be("0");
    }

    [TestMethod]
    public async Task RandDistribution()
    {
        //ensure we didn't get any fractional values
        var query = """
                    range i from 1 to 1000 step 1 
                    | extend r=rand(100)
                    |summarize m=min(r),x=max(r)
                    | extend rg = x-m
                    | project rg

                    """;
        var result = await LastLineOfResult(query);
        //check good distribution
        double.Parse(result).Should().BeInRange(90, 100);
    }

    [TestMethod]
    public async Task Rand_NegativeMax_ShouldReturnErrorOrNull()
    {
        var query = "print c=rand(-5)";
        var result = await LastLineOfResult(query);
        var r = double.Parse(result);
        r.Should().BeInRange(0, 1);
    }

    [TestMethod]
    public async Task RandPrint()
    {
        var query = "print c=rand(1)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("0");
    }

    [TestMethod]
    public async Task RandParam()
    {
        //ensure we didn't get any fractional values
        var query = """
                    range i from 1 to 1000 step 1 
                    | extend r=rand(i)
                    |summarize m=min(r),x=max(r)
                    | extend rg = x-m
                    | project rg
                    """;
        var result = await LastLineOfResult(query);
        //check good distribution
        double.Parse(result).Should().BeInRange(10, 990);
    }
}
