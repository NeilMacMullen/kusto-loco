using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class MakeSeriesOperatorTests : TestMethods
{
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
