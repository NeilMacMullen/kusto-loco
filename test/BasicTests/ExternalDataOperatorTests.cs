using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using KustoLoco.Core;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class ExternalDataOperatorTests : TestMethods
{
    private sealed class StubResolver : IExternalDataResolver
    {
        public IReadOnlyList<object?[]>? Resolve(ExternalDataRequest request) =>
            new object?[][]
            {
                new object?[] { "alice", "bob" }, // name column
                new object?[] { 30L, 40L },       // age column
            };
    }

    private static string LastLine(KustoQueryResult result) =>
        result.GetRow(result.RowCount - 1).Select(KustoFormatter.ObjectToKustoString).JoinString();

    [TestMethod]
    public async Task ExternalData_WithResolver_ProducesRows()
    {
        var context = CreateContext();
        context.AddProvider<IExternalDataResolver>(new StubResolver());
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.csv'] with(format='csv')");
        result.RowCount.Should().Be(2);
        LastLine(result).Should().Contain("bob").And.Contain("40");
    }

    [TestMethod]
    public async Task ExternalData_WithResolver_CanBePipelined()
    {
        var context = CreateContext();
        context.AddProvider<IExternalDataResolver>(new StubResolver());
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.csv'] with(format='csv') " +
            "| where age > 35 | project name");
        result.RowCount.Should().Be(1);
        LastLine(result).Should().Be("bob");
    }

    [TestMethod]
    public async Task ExternalData_NoResolver_FailsClosedEmpty()
    {
        var context = CreateContext(); // no resolver registered
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.csv'] with(format='csv')");
        result.RowCount.Should().Be(0);
    }
}
