using System;
using System.Collections.Generic;
using System.Linq;
using AwesomeAssertions;
using KustoLoco.Core;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class ExternalDataOperatorTests : TestMethods
{
    /// <summary>A host resolver that serves canned text, exercising the real path: the host returns delimited rows,
    /// the engine types them per the declared schema.</summary>
    private sealed class StubResolver : IExternalDataResolver
    {
        private readonly string _content;
        public StubResolver(string content) => _content = content;
        public IReadOnlyList<IReadOnlyList<string>> ResolveRows(string uri, string format) =>
            DelimitedTextParser.Parse(_content, DelimitedTextParser.DelimiterFor(format));
    }

    private const string Csv = "alice,30\nbob,40\n";

    private static string LastLine(KustoQueryResult result) =>
        result.GetRow(result.RowCount - 1).Select(KustoFormatter.ObjectToKustoString).JoinString();

    [TestMethod]
    public async Task ExternalData_WithResolver_ProducesTypedRows()
    {
        var context = CreateContext().SetExternalDataResolver(new StubResolver(Csv));
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.csv'] with(format='csv')");
        result.RowCount.Should().Be(2);
        LastLine(result).Should().Contain("bob").And.Contain("40");
    }

    [TestMethod]
    public async Task ExternalData_WithResolver_CanBePipelined()
    {
        // 'age' must be a real long for the predicate to work — proof the engine typed the text cells.
        var context = CreateContext().SetExternalDataResolver(new StubResolver(Csv));
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.csv'] with(format='csv') " +
            "| where age > 35 | project name");
        result.RowCount.Should().Be(1);
        LastLine(result).Should().Be("bob");
    }

    [TestMethod]
    public async Task ExternalData_HonoursTheDeclaredFormat()
    {
        var context = CreateContext().SetExternalDataResolver(new StubResolver("alice\t30\nbob\t40\n"));
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.tsv'] with(format='tsv') " +
            "| where age > 35 | project name");
        LastLine(result).Should().Be("bob");
    }

    [TestMethod]
    public async Task ExternalData_NoResolver_FailsLoud()
    {
        // An unreachable list must NEVER look like a clean no-match: with no resolver the query reports an ERROR
        // (the engine captures query errors into the result) rather than quietly yielding zero rows.
        var context = CreateContext(); // no resolver registered
        var result = await context.RunQuery(
            "externaldata(name:string, age:long)['https://example.com/data.csv'] with(format='csv')");
        result.Error.Should().NotBeEmpty();
        result.Error.Should().Contain("IExternalDataResolver");
    }

    [TestMethod]
    public async Task ExternalData_ResolverFailure_IsReportedNotSwallowed()
    {
        var context = CreateContext().SetExternalDataResolver(new ThrowingResolver());
        var result = await context.RunQuery(
            "externaldata(name:string)['https://example.com/data.csv'] with(format='csv')");
        result.Error.Should().NotBeEmpty();
        result.RowCount.Should().Be(0);
    }

    private sealed class ThrowingResolver : IExternalDataResolver
    {
        public IReadOnlyList<IReadOnlyList<string>> ResolveRows(string uri, string format) =>
            throw new NotSupportedException("host refuses this URI");
    }
}
