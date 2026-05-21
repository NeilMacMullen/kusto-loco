using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class ModuloTests:TestMethods
{
    [TestMethod]
    public async Task ModuloInt()
    {
        var query = "print result = 10 % 3";
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }

    [TestMethod]
    public async Task ModuloLong()
    {
        var query = "print result = 100 % 7";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task ModuloReal()
    {
        var query = "print result = 10.5 % 3.0";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("1.5");
    }

    [TestMethod]
    public async Task ModuloNegativeInt()
    {
        var query = "print result = -10 % 3";
        var result = await LastLineOfResult(query);
        result.Should().Be("-1");
    }

    [TestMethod]
    public async Task ModuloNegativeDivisor()
    {
        var query = "print result = 10 % -3";
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }

    [TestMethod]
    public async Task ModuloTimeSpanByTimeSpan()
    {
        var query = "print result = 10d % 3d";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.00:00:00");
    }

    [TestMethod]
    public async Task ModuloTimeSpanByTimeSpanHours()
    {
        var query = "print result = 25h % 7h";
        var result = await LastLineOfResult(query);
        result.Should().Be("04:00:00");
    }

 
    [TestMethod]
    public async Task ModuloIntByLong()
    {
        var query = "print result = toint(17) % 5";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task ModuloLongByInt()
    {
        var query = "print result = 17 % toint(5)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task ModuloRealByInt()
    {
        var query = "print result = 17.5 % toint(5)";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("2.5");
    }

    [TestMethod]
    public async Task ModuloIntByReal()
    {
        var query = "print result = toint(17) % 5.0";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("2");
    }

    [TestMethod]
    public async Task ModuloInDataTable()
    {
        var query = "datatable(a:long, b:long) [10,3, 15,4, 20,6] | project result = a % b";
        var result = await LastLineOfResult(query);
        result.Should().Be("2");
    }

    [TestMethod]
    public async Task ModuloTimeSpanInDataTable()
    {
        var query = "datatable(a:timespan, b:timespan) [10d,3d, 15d,4d] | project result = a % b";
        var result = await LastLineOfResult(query);
        result.Should().Be("3.00:00:00");
    }

    [TestMethod]
    public async Task ModuloZeroDivisor()
    {
        var query = "print result = 10 % 0";
        var result = await LastLineOfResult(query);
        // This may return null, infinity, or an error depending on implementation
        // Just verify the query completes
        result.Should().NotBeNull();
    }

    [TestMethod]
    public async Task ModuloWithBinOperation()
    {
        var query = "print result = (10 + 5) % 4";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task ModuloChained()
    {
        var query = "print result = 100 % 30 % 7";
        var result = await LastLineOfResult(query);
        result.Should().Be("3");
    }

    [TestMethod]
    public async Task ModuloRealPrecision()
    {
        var query = "print result = 7.5 % 2.3";
        var result = await LastLineOfResult(query);
        // 7.5 % 2.3 = 7.5 - 3*2.3 = 7.5 - 6.9 = 0.6
        result.Should().StartWith("0.6");
    }

    [TestMethod]
    public async Task ModuloDateTimeByTimeSpan()
    {
        var query = "print result = datetime(2024-01-15 08:30:00) % 1d";
        var result = await LastLineOfResult(query);
        // The result should be the time portion (08:30:00) since we're taking modulo of datetime by 1 day
        result.Should().Be("08:30:00");
    }

    [TestMethod]
    public async Task ModuloDateTimeByTimeSpanHours()
    {
        var query = "print result = datetime(2024-01-15 14:45:30) % 12h";
        var result = await LastLineOfResult(query);
        // 14:45:30 % 12h = 2:45:30
        result.Should().Be("02:45:30");
    }


    [TestMethod]
    public async Task ModuloDateTimeByTimeSpanBackToUnixDateTime()
    {
        var query = "print unix_time(datetime(2024-01-15 14:45:30) % 12h)";
        var result = await LastLineOfResult(query);
        // 14:45:30 % 12h = 2:45:30
        result.Should().Contain("1970-01-01 02:45:30");
    }
    [TestMethod]
    public async Task ModuloDateTimeByTimeSpanBackToNetDateTime()
    {
        var query = "print net_time(datetime(2024-01-15 14:45:30) % 12h)";
        var result = await LastLineOfResult(query);
        // 14:45:30 % 12h = 2:45:30
        result.Should().Contain("0001-01-01 02:45:30");
    }

    [TestMethod]
    public async Task ModuloDateTimeByTimeSpanInDataTable()
    {
        var query = "datatable(dt:datetime, ts:timespan) [datetime(2024-01-15 10:30:00), 1d, datetime(2024-02-20 15:45:00), 12h] | project result = dt % ts";
        var result = await LastLineOfResult(query);
        // 15:45:00 % 12h = 3:45:00
        result.Should().Be("03:45:00");
    }

}
