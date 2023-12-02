using FluentAssertions;
using KustoSupport;
using LogSetup;
using NLog;

namespace BasicTests;

[TestClass]
public class ChunkTests
{
    public ChunkTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    private static KustoQueryContext CreateContext() => KustoQueryContext.WithFullDebug();

    [TestMethod]
    public async Task Take()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        context.AddChunkedTableFromRecords("data", rows, 2);
        var result = (await context.RunQuery("data | take 5"));
        result.Results.Count.Should().Be(5);
        //TODO - here compare tabulated output for more coverage
    }


    [TestMethod]
    public async Task Count()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        context.AddChunkedTableFromRecords("data", rows, 2);
        var result = (await context.RunQuery("data | count"));
        KustoFormatter.Tabulate(result.Results).Should().Contain("20");
    }

    [TestMethod]
    public async Task WhereCount()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        context.AddChunkedTableFromRecords("data", rows, 2);
        var result = (await context.RunQuery("data | where Value < 10 | count"));
        KustoFormatter.Tabulate(result.Results).Should().Contain("10");
    }
}