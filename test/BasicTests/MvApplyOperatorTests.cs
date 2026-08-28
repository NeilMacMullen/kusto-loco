using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class MvApplyOperatorTests : TestMethods
{
    [TestMethod]
    public async Task MvApply_SummarizePerSourceRow()
    {
        // Each source row's array is summed independently: [1,2,3]->6, [10,20]->30.
        var query =
            "datatable(id:long, vals:dynamic)[1, dynamic([1,2,3]), 2, dynamic([10,20])] " +
            "| mv-apply vals to typeof(long) on (summarize s=sum(vals))";
        var result = await ResultAsString(query, ";");
        result.Should().Contain("6").And.Contain("30");
    }

    [TestMethod]
    public async Task MvApply_FilterPerRow()
    {
        // Expand [1,2,3,4], keep vals>2 -> 3,4.
        var query =
            "datatable(id:long, vals:dynamic)[1, dynamic([1,2,3,4])] " +
            "| mv-apply vals to typeof(long) on (where vals > 2 | project vals)";
        var result = await ResultAsString(query, ";");
        // id=1 survives; only vals 3 and 4 pass the filter (1 and 2 are dropped).
        result.Should().Contain("1,3").And.Contain("1,4");
        result.Should().NotContain("1,2");
    }

    [TestMethod]
    public async Task MvApply_SourceColumnSurvives()
    {
        // The source 'id' survives the subquery and is associated with each output row.
        var query =
            "datatable(id:long, vals:dynamic)[7, dynamic([1,2,3])] " +
            "| mv-apply vals to typeof(long) on (summarize s=sum(vals))";
        var result = await ResultAsString(query, ";");
        result.Should().Contain("7").And.Contain("6");
    }
}
