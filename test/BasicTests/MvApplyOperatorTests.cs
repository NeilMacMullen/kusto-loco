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
    public async Task MvApply_ExpandsAnAliasedExpression()
    {
        // The expanded item may be an ALIASED EXPRESSION rather than an existing column — the common real-world
        // form, e.g. mv-apply p = parse_json(Col) on (...). 'p' is not in the input schema, so the operator must
        // evaluate the expression rather than resolve the name against the source row.
        var query =
            "datatable(id:long, payload:string)[1, '[1,2,3]'] " +
            "| mv-apply p = parse_json(payload) to typeof(long) on (summarize s=sum(p))";
        var result = await ResultAsString(query, ";");
        result.Should().Contain("6");
    }

    [TestMethod]
    public async Task MvApply_AliasedExpression_NameValuePairsIntoABag()
    {
        // The name/value-pair idiom: expand a dynamic array of {Name,Value} objects and fold it into one bag per
        // source row. Exercises an aliased expression + dynamic member access + an aggregate over the expansion.
        var query =
            "datatable(id:long, Parameters:string)[1, '[{\"Name\":\"Alpha\",\"Value\":\"1\"},{\"Name\":\"Beta\",\"Value\":\"2\"}]'] " +
            "| mv-apply p = parse_json(Parameters) on ( summarize P = make_bag(bag_pack(tolower(tostring(p.Name)), p.Value)) )";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("alpha").And.Contain("beta");
    }

    [TestMethod]
    public async Task MvApply_EmptyArrayProducesNoRowsForThatRecord()
    {
        // mv-apply is a lateral join: a record whose array is empty contributes nothing. (mv-expand differs — it
        // keeps the record with a null.) Running the subquery over an empty expansion would resurrect the record,
        // because an aggregate over an empty table still emits a row.
        var query =
            "datatable(id:long, payload:string)[1, '[]', 2, '[1,2]'] " +
            "| mv-apply p = parse_json(payload) to typeof(long) on (summarize s=sum(p)) " +
            "| project id";
        var result = await CreateContext().RunQuery(query);
        result.RowCount.Should().Be(1);            // only id=2 survives
        var rendered = await ResultAsString(query, ";");
        rendered.Should().Contain("2").And.NotContain("1;");
    }

    [TestMethod]
    public async Task MvApply_BagThenReadBackAField()
    {
        // The full real-world round trip: fold name/value pairs into a bag, then read a field back out of it —
        // which is what makes the idiom worth using at all.
        var query =
            "datatable(id:long, Parameters:string)" +
            "[1, '[{\"Name\":\"ForwardTo\",\"Value\":\"evil@x.com\"},{\"Name\":\"Enabled\",\"Value\":\"True\"}]'] " +
            "| mv-apply p = parse_json(Parameters) on ( summarize P = make_bag(bag_pack(tolower(tostring(p.Name)), p.Value)) ) " +
            "| extend Target = tostring(P['forwardto']) " +
            "| project Target";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("evil@x.com");
    }

    [TestMethod]
    public async Task MvApply_ExpansionFeedsMakeSet()
    {
        // mv-apply feeding make_set — the aggregate most used by real rules.
        var query =
            "datatable(id:long, payload:string)[1, '[\"a\",\"b\",\"a\"]'] " +
            "| mv-apply p = parse_json(payload) on ( summarize S = make_set(tostring(p)) )";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("a").And.Contain("b");
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
