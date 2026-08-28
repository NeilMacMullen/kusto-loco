using System.Linq;
using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class MakeSeriesOperatorTests : TestMethods
{
    [TestMethod]
    public async Task MakeSeries_ColumnOrder_AggregateFirst_AxisLast_DateTimeExact()
    {
        // Kusto schema order is [aggregates..., axis]; the axis is the LAST column. The datetime axis must be exact
        // (computed in ticks, not double), and with no 'default=' the empty Jan-02 bin fills with 0.
        var q = "datatable(t:datetime, v:long)[datetime(2017-01-01),10, datetime(2017-01-03),30] " +
                "| make-series s=sum(v) on t from datetime(2017-01-01) to datetime(2017-01-04) step 1d";
        var result = await Result(q);
        result.ColumnDefinitions().Select(c => c.Name).Should().Equal("s", "t");
        var row = result.GetRow(0).ToArray();
        Squash(row[0]?.ToString() ?? "").Should().Be("[10,0,30]");           // aggregate series, default-0 gap fill
        var axis = row[1]?.ToString() ?? "";
        axis.Should().Contain("2017-01-01T00:00:00.0000000Z")
            .And.Contain("2017-01-03T00:00:00.0000000Z")
            .And.NotContain("2017-01-04");                                   // 'to' is non-inclusive
    }

    [TestMethod]
    public async Task MakeSeries_NonMultipleRange_HasCorrectPointCount()
    {
        // from 0 to 11 step 2 -> 0,2,4,6,8,10 (11 non-inclusive) = 6 points. A floor() count would give only 5.
        var q = "datatable(x:long)[0,2,4,6,8,10] | make-series c=count() on x from 0 to 11 step 2";
        var result = await SquashedLastLineOfResult(q);
        result.Should().Contain("[1,1,1,1,1,1]");
    }

    [TestMethod]
    public async Task MakeSeries_NoExplicitDefault_FillsZeroNotNull()
    {
        // No 'default=' clause: ADX fills empty bins with a typed 0, not null.
        var q = "datatable(x:long)[1,3] | make-series c=count() on x from 1 to 4 step 1";
        var result = await SquashedLastLineOfResult(q);
        result.Should().Contain("[1,0,1]").And.NotContain("null");
    }

    [TestMethod]
    public async Task MakeSeries_SumOverNumericAxis()
    {
        var query = "datatable(x:long, val:long)[1,10, 2,20, 3,30] " +
                    "| make-series s=sum(val) default=0 on x from 1 to 4 step 1";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("[10,20,30]");
    }

    [TestMethod]
    public async Task MakeSeries_GapFillsEmptyBucketsWithDefault()
    {
        // x has 1 and 3 but not 2 -> the middle bucket is empty and gap-filled with 0.
        var query = "datatable(x:long, val:long)[1,10, 3,30] " +
                    "| make-series s=sum(val) default=0 on x from 1 to 4 step 1";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("[10,0,30]");
    }

    [TestMethod]
    public async Task MakeSeries_ByGroup()
    {
        var query = "datatable(g:string, x:long, val:long)['a',1,10, 'a',2,20, 'b',1,100] " +
                    "| make-series s=sum(val) default=0 on x from 1 to 3 step 1 by g";
        // Last group is 'b': x=1 -> 100, x=2 empty -> gap-filled 0.
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("b").And.Contain("[100,0]");
    }

    [TestMethod]
    public async Task MakeSeries_CountAggregate()
    {
        var query = "datatable(x:long)[1, 1, 2] " +
                    "| make-series c=count() default=0 on x from 1 to 3 step 1";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("[2,1]");
    }
}
