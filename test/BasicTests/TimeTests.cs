using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class TimeTests : TestMethods
{
    [TestMethod]
    public async Task UnixSeconds()
    {
        var query = """
                        print date_time = unixtime_seconds_todatetime(1546300800)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("2019-01-01 00:00:00");
    }

    [TestMethod]
    public async Task UnixMilliSeconds()
    {
        var query = """
                     print date_time = unixtime_milliseconds_todatetime(1546300800000)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("2019-01-01 00:00:00");
    }



    [TestMethod]
    public async Task UnixMicroSeconds()
    {
        var query = """
                    print date_time = unixtime_microseconds_todatetime(1546300800000000)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("2019-01-01 00:00:00");
    }


    [TestMethod]
    public async Task UnixNanoSeconds()
    {
        var query = """
                    print date_time = unixtime_nanoseconds_todatetime(1546300800000000000)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Contain("2019-01-01 00:00:00");
    }
}
