using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class BinTests : TestMethods
{
    [TestMethod]
    public async Task Long()
    {
        var query = "print bin_at(13, 10, 11)";
        var result = await LastLineOfResult(query);
        result.Should().Be("11");
    }


    [TestMethod]
    public async Task Double()
    {
        var query = "print bin_at(6.5, 2.5, 7)";
        var result = await LastLineOfResult(query);
        result.Should().Be("4.5");
    }
    [TestMethod]
    public async Task Durations()
    {
        var query = "print bin_at(time(1h), 1d, 12h)";
        var result = await LastLineOfResult(query);
        result.Should().Be("-12:00:00");
    }

    [TestMethod]
    public async Task DateTimes()
    {
        var query = "print bin_at(datetime(2017-05-17 10:20:00.0), 7d, datetime(2017-06-04 00:00:00.0))";
        var result = await LastLineOfResult(query);
        result.Should().Contain("2017-05-14");
    }

    [TestMethod]
    public async Task SimpleDateTime()
    {
        var query = """
                    print bin_at(datetime(2018-02-23T16:14),1d,datetime(2018-02-24 15:14:00.0000000))
                    """;
        var result = await ResultAsString(query,Environment.NewLine);
        result.Should().Be("""
                           2018-02-23 15:14:00Z
                           """);
    }
    
    [TestMethod]
    public async Task Table()
    {
        var query = """
                    datatable(Date:datetime, NumOfEvents:int)[
                    datetime(2018-02-23T16:14),4,
                    datetime(2018-02-23T17:29),4,
                    datetime(2018-02-24T15:14),3,
                    datetime(2018-02-24T15:24),4,
                    datetime(2018-02-26T15:14),5]
                    | summarize TotalEvents=sum(NumOfEvents) by D=bin_at(Date, 1d, datetime(2018-02-24 15:14:00.0000000))
                    | order by D asc
                    """;
        var result = await ResultAsString(query,Environment.NewLine);
        result.Should().Be("""
                           2018-02-23 15:14:00Z,8
                           2018-02-24 15:14:00Z,7
                           2018-02-26 15:14:00Z,5
                           """);
    }

}
