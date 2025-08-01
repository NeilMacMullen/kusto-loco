using System.Globalization;
using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class DateTimeTests : TestMethods
{
    public DateTimeTests()
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
    }

    [TestMethod]
    public async Task InvalidDateTimeFormat_ShouldReturnNull()
    {
        var query = "print D=todatetime('not-a-date')";
        var result = await LastLineOfResult(query);
        result.Should().Be(""); // KQL returns null for invalid datetime parse
    }


    [TestMethod]
    public async Task MakeDateTime_InvalidMonth_ShouldReturnNull()
    {
        var query = "print c=make_datetime(2020, 13, 1)";
        var result = await LastLineOfResult(query);
        result.Should().Be(""); // Invalid month should return null
    }

    [TestMethod]
    public async Task DateTimeBin()
    {
        var query = "print bin(datetime(1970-05-11 13:45:07), 1d)";
        var result = await LastLineOfResult(query);
        result.Should().Be("1970-05-11 00:00:00Z");
    }


    [TestMethod]
    public async Task BetweenDateTime()
    {
        //ensure we didn't get any fractional values
        var query = """

                    range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
                    | where x between (datetime(2023-01-10) .. datetime(2023-01-15))
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-15 00:00:00Z");
    }

    [TestMethod]
    public async Task DateTimeToLocal()
    {
        var query = "print datetime_utc_to_local(datetime(2015-12-31 23:59:59.9), 'US/Eastern')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2015-12-31 18:59:59.9000");
    }


    [TestMethod]
    public async Task BetweenDateTimeTimespan()
    {
        //ensure we didn't get any fractional values
        var query = """

                    range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
                    | where x between (datetime(2023-01-10) .. 3d)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-13 00:00:00Z");
    }

    [TestMethod]
    public async Task ToDateTime()
    {
        var query = "print D=todatetime('15/01/2024 12:35:35')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2024-01-15 12:35:35Z");
    }

    [TestMethod]
    public async Task ToDateTime3()
    {
        var query = "print todatetime('13-02-2022') == datetime(13-02-2022)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task DateTime4()
    {
        var query = "print datetime(2022-02-12)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("2022");
    }

    [TestMethod]
    public async Task ToDateTime2()
    {
        var query = "print D=todatetime('2024/01/15 12:35:35')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2024-01-15 12:35:35Z");
    }

    [TestMethod]
    public async Task ToDateTimeFmt()
    {
        var query = "print D=todatetimefmt('2024-01-15 12:35:35','yyyy-MM-dd HH:mm:ss')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2024-01-15 12:35:35Z");
    }

    [TestMethod]
    public async Task AvgDateTime()
    {
        var query = "datatable(a:datetime) [datetime(2023-06-10),datetime(2023-06-12)] | summarize avg(a)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("2023-06-11");
    }

    [TestMethod]
    public async Task MakeDateTime()
    {
        var query = "print make_datetime(2000,4,15,1,1)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2000-04-15 01:01:00Z");
    }


    [TestMethod]
    public async Task MakeDateTime2()
    {
        var query = "print make_datetime(2000,4,15)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2000-04-15 00:00:00Z");
    }


    [TestMethod]
    public async Task DatetimeAdd() =>
        (await LastLineOfResult("print datetime_add('year',-5,make_datetime(2017,1,1))"))
        .Should().Be("2012-01-01 00:00:00Z");

    [TestMethod]
    public async Task DatetimeDiff()
    {
        (await LastLineOfResult("print  datetime_diff('year',datetime(2017-01-01),datetime(2000-12-31))"))
            .Should().Be("17");
        (await LastLineOfResult("print datetime_diff('quarter',datetime(2017-07-01),datetime(2017-03-30))"))
            .Should().Be("2");
        (await LastLineOfResult("print datetime_diff('month',datetime(2017-01-01),datetime(2015-12-30))"))
            .Should().Be("13");
        (await LastLineOfResult("print datetime_diff('week',datetime(2017-10-29 00:00),datetime(2017-09-30 23:59))"))
            .Should().Be("5");
        (await LastLineOfResult("print datetime_diff('day',datetime(2017-10-29 00:00),datetime(2017-09-30 23:59))"))
            .Should().Be("29");
        (await LastLineOfResult("print datetime_diff('hour',datetime(2017-10-31 01:00),datetime(2017-10-30 23:59))"))
            .Should().Be("2");
        (await LastLineOfResult(
                "print datetime_diff('minute',datetime(2017-10-30 23:05:01),datetime(2017-10-30 23:00:59))")).Should()
            .Be("5");
        (await LastLineOfResult(
                "print datetime_diff('second',datetime(2017-10-30 23:00:10.100),datetime(2017-10-30 23:00:00.900))"))
            .Should().Be("10");
        (await LastLineOfResult(
                "print datetime_diff('millisecond',datetime(2017-10-30 23:00:00.200100),datetime(2017-10-30 23:00:00.100900))"))
            .Should().Be("100");
        (await LastLineOfResult(
                "print datetime_diff('microsecond',datetime(2017-10-30 23:00:00.1009001),datetime(2017-10-30 23:00:00.1008009))"))
            .Should().Be("100");
        (await LastLineOfResult(
                "print datetime_diff('nanosecond',datetime(2017-10-30 23:00:00.0000000),datetime(2017-10-30 23:00:00.0000007))"))
            .Should().Be("-700");
    }

    [TestMethod]
    public async Task RangeDateTime()
    {
        //ensure we didn't get any fractional values
        var query = """

                    range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-30 00:00:00Z");
    }

    [TestMethod]
    public async Task RangeDateTimeFiltered()
    {
        //ensure we didn't get any fractional values
        var query = """

                    range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
                    | where x > datetime(2022-01-01)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-30 00:00:00Z");
    }
}
