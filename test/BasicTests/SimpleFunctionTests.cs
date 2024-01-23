using FluentAssertions;
// ReSharper disable StringLiteralTypo

namespace BasicTests;

[TestClass]
public class SimpleFunctionTests : TestMethods
{
    [TestMethod]
    public async Task TrimStart()
    {
        var query = "print trim_start(@'a+','aaainnerbbba')";
        var result = await LastLineOfResult(query);
        result.Should().Be("innerbbba");
    }

    [TestMethod]
    public async Task TrimEnd()
    {
        var query = "print trim_end(@'b+','baaainnerbbb')";
        var result = await LastLineOfResult(query);
        result.Should().Be("baaainner");
    }

    [TestMethod]
    public async Task Trim()
    {
        var query = "print trim(@'a+','aaaainneraaaa')";
        var result = await LastLineOfResult(query);
        result.Should().Be("inner");
    }

    [TestMethod]
    public async Task Case()
    {
        var query = @" 
datatable(Size:int) [7] 
| extend S= case(Size <= 3, 'Small',                        
                 Size <= 10, 'Medium', 
                             'Large')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("Medium");
    }

    [TestMethod]
    public async Task CaseDefault()
    {
        var query = @" 
datatable(Size:int) [50] 
|extend S= case(Size <= 3, 'Small',                        
              Size <= 10, 'Medium', 
                          'Large')";
        var result = await LastLineOfResult(query);
        result.Should().Contain("Large");
    }

    [TestMethod]
    public async Task GeoDistance2PointsScalar()
    {
        var query = " print distance_in_meters = geo_distance_2points(-122.407628, 47.578557, -118.275287, 34.019056)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("15467");
    }

    [TestMethod]
    public async Task GeoPointToGeoHashScalar()
    {
        var query = "print geohash = geo_point_to_geohash(139.806115, 35.554128, 12)";
        var result = await LastLineOfResult(query);
        result.Should().Contain("xn76m27ty9g4");
    }

    [TestMethod]
    public async Task GeoPointToGeoHashScalarWithDefault()
    {
        var query = "print geohash = geo_point_to_geohash(139.806115, 35.554128)";
        var result = await LastLineOfResult(query);
        result.Should().Be("xn76m");
    }


    [TestMethod]
    public async Task GeoHashToCentralPointScalar()
    {
        var query = @"print point = geo_geohash_to_central_point('sunny')
                      | extend coordinates = point.coordinates
                      | project longitude = coordinates[0]";
        var result = await LastLineOfResult(query);
        result.Should().Contain("42.4731445");
    }


    [TestMethod]
    public async Task SplitScalar()
    {
        var query = "print c=split('this.is.a.string.and.I.need.the.last.part', '.')[-1]";
        var result = await LastLineOfResult(query);
        result.Should().Be("part");
    }


    [TestMethod]
    public async Task ToLower()
    {
        var query = "print c=tolower('ABCdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abcdef");
    }

    [TestMethod]
    public async Task ToUpper()
    {
        var query = "print c=toupper('ABCdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("ABCDEF");
    }

    [TestMethod]
    public async Task Strlen()
    {
        var query = "print c=strlen('ABCdef')";
        var result = await LastLineOfResult(query);
        result.Should().Be("6");
    }

    [TestMethod]
    public async Task SubString()
    {
        var query = "print c=substring('ABCdef',2,3)";
        var result = await LastLineOfResult(query);
        result.Should().Be("Cde");
    }

    [TestMethod]
    public async Task Trimws()
    {
        var query = "print c=trimws('   abc   ')";
        var result = await LastLineOfResult(query);
        result.Should().Be("abc");
    }

    [TestMethod]
    public async Task DateTimeBin()
    {
        var query = "print bin(datetime(1970-05-11 13:45:07), 1d)";
        var result = await LastLineOfResult(query);
        result.Should().Be("1970-05-11 00:00:00Z");
    }

    [TestMethod]
    public async Task TimespanFormatting()
    {
        var query = "print 1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("1.00:00:00");
    }

    [TestMethod]
    public async Task DateTimeToLocal()
    {
        var query = "print datetime_utc_to_local(datetime(2015-12-31 23:59:59.9), 'US/Eastern')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2015-12-31 18:59:59.9000");
    }


    [TestMethod]
    public async Task Range()
    {
        var query = "range i from 1 to 10 step 1";
        var result = await LastLineOfResult(query);
        result.Should().Be("10");
    }

    [TestMethod]
    public async Task RangeDescending()
    {
        var query = "range i from 10 to 1 step -1";
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }


    [TestMethod]
    public async Task RowNumberNoParam()
    {
        var query = "range i from 10 to 20 step 1 | extend r =row_number()";
        var result = await LastLineOfResult(query);
        result.Should().Be("20,10");
    }

    [TestMethod]
    public async Task RowNumberStartingAt7()
    {
        var query = "range i from 1 to 5 step 1 | extend r =row_number(7)";
        var result = await LastLineOfResult(query);
        result.Should().Be("5,11");
    }

    [TestMethod]
    public async Task RowNumberWithRanking()
    {
        var query = @"range i from 1 to 100 step 1 
| extend r =row_number(1,i%10==0) 
| where r==1 
| count";
        var result = await LastLineOfResult(query);
        result.Should().Be("11");
    }

    [TestMethod]
    public async Task Rand()
    {
        //difficult to test randomness but ensure we got
        //5 different values and they were all <1
        var query = @"range i from 1 to 5 step 1 
| extend r =rand()
| where r >=0 and r <1
| summarize by r | count ";
        var result = await LastLineOfResult(query);
        result.Should().Be("5");
    }

    [TestMethod]
    public async Task RandInt()
    {
        //ensure we didn't get any fractional values
        var query = @"range i from 1 to 5 step 1 
| extend r =rand(100)
| where toint(r) != r
| count ";
        var result = await LastLineOfResult(query);
        result.Should().Be("0");
    }


    [TestMethod]
    public async Task BetweenLong()
    {
        //ensure we didn't get any fractional values
        var query = @"range x from 1 to 100 step 1
| where x between (50 .. 55)";
        var result = await LastLineOfResult(query);
        result.Should().Be("55");
    }

    [TestMethod]
    public async Task BetweenInt()
    {
        //ensure we didn't get any fractional values
        var query = @"range i from 1 to 100 step 1
| extend i=toint(i) | where i between (50 .. 55)";
        var result = await LastLineOfResult(query);
        result.Should().Be("55");
    }

    [TestMethod]
    public async Task RangeDateTime()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-30 00:00:00Z");
    }

    [TestMethod]
    public async Task RangeDateTimeFiltered()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
| where x > datetime(2022-01-01)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-30 00:00:00Z");
    }

    [TestMethod]
    public async Task RangeTimeSpan()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from 1d to 20d step 1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("20.00:00:00");
    }

    [TestMethod]
    public async Task RangeTimeSpanFiltered()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from 1d to 20d step 1d | where x < 5d ";
        var result = await LastLineOfResult(query);
        result.Should().Be("4.00:00:00");
    }

    [TestMethod]
    public async Task BetweenDateTime()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
| where x between (datetime(2023-01-10) .. datetime(2023-01-15))";
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-15 00:00:00Z");
    }

    [TestMethod]
    public async Task BetweenDateTimeTimespan()
    {
        //ensure we didn't get any fractional values
        var query = @"
range x from datetime(2023-01-01) to datetime(2023-01-30) step 1d
| where x between (datetime(2023-01-10) .. 3d)";
        var result = await LastLineOfResult(query);
        result.Should().Be("2023-01-13 00:00:00Z");
    }

    [TestMethod]
    public async Task NotBetweenLong()
    {
        //ensure we didn't get any fractional values
        var query = @"range x from 1 to 10 step 1
| where x !between (9 .. 11)";
        var result = await LastLineOfResult(query);
        result.Should().Be("8");
    }

    [TestMethod]
    [Ignore("not yet implemented")]
    public async Task HasAny()
    {
        //ensure we didn't get any fractional values
        var query = "print x='line1' | where x has_any('a','b')";
        var result = await LastLineOfResult(query);
        result.Should().Be("8");
    }


    [TestMethod]
    [Ignore("not yet implemented")]
    public async Task Arg_max()
    {
        //ensure we didn't get any fractional values
        var query = "print x=1,y=2 | summarize arg_max(x,*) by y"
            ;
        var result = await LastLineOfResult(query);
        result.Should().Be("8");
    }

    [TestMethod]
    public async Task LogTest()
    {
        var query = "print v1 = log(4.5)";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("1.504");
    }

    [TestMethod]
    public async Task LogTest2()
    {
        var query = "datatable(a:real) [4.5,4.5] | project v1 = log(a)";
        var result = await LastLineOfResult(query);
        result.Should().StartWith("1.504");
    }

    [TestMethod]
    public async Task StrLen()
    {
        var query = "datatable(a:string) [''] | project v1 = strlen(a)";
        var result = await LastLineOfResult(query);
        result.Should().Be("0");
    }


    [TestMethod]
    public async Task ToDateTime()
    {
        var query = "print D=todatetime('15/01/2024 12:35:35')";
        var result = await LastLineOfResult(query);
        result.Should().Be("2024-01-15 12:35:35Z");
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
    public async Task MultiplyTimespan()
    {
        var query = "print D=1d * 10";
        var result = await LastLineOfResult(query);
        result.Should().Be("10.00:00:00");
    }

    [TestMethod]
    public async Task MultiplyTimespanR()
    {
        var query = "print D=10*1d";
        var result = await LastLineOfResult(query);
        result.Should().Be("10.00:00:00");
    }

    [TestMethod]
    public async Task AbsInt()
    {
        var query = "print D=abs(toint(-99))";
        var result = await LastLineOfResult(query);
        result.Should().Be("99");
    }
}