using FluentAssertions;
using KustoLoco.Core;
using KustoLoco.Core.DataSource;
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

    private static KustoQueryContext CreateContext() => KustoQueryContext.CreateWithDebug();

    private void AddChunkedTableFromRecords<T>(KustoQueryContext context,string tableName, IReadOnlyCollection<T> records, int chunkSize)
    {
        var table = TableBuilder.CreateFromVolatileData(tableName, records);
        var chunked = ChunkedKustoTable
            .FromTable(table.ToTableSource(), chunkSize);
        context.AddTable(chunked);
    }

    [TestMethod]
    public async Task Take()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context,"data", rows, 2);
        var result = (await context.RunTabularQueryAsync("data | take 5"));
        result.Height.Should().Be(5);
        //TODO - here compare tabulated output for more coverage
    }


    [TestMethod]
    public async Task Count()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context,"data", rows, 2);
        var result = (await context.RunTabularQueryAsync("data | count"));
        KustoFormatter.Tabulate(result).Should().Contain("20");
    }

    [TestMethod]
    public async Task WhereCount()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 2);
        var result = (await context.RunTabularQueryAsync("data | where Value < 10 | count"));
        KustoFormatter.Tabulate(result).Should().Contain("10");
    }
}