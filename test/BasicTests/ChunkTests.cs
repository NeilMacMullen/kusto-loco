using FluentAssertions;
using KustoLoco.Core;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Util;
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

    private static KustoQueryContext CreateContext()
        => KustoQueryContext.CreateForTest();

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
        var result = (await context.RunQuery("data | take 5"));
        result.RowCount.Should().Be(5);
        //TODO - here compare tabulated output for more coverage
    }
    

    [TestMethod]
    public async Task Count()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context,"data", rows, 2);
        var result = (await context.RunQuery("data | count"));
        KustoFormatter.Tabulate(result).Should().Contain("20");
    }

    [TestMethod]
    public async Task WhereCount()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 2);
        var result = (await context.RunQuery("data | where Value < 10 | count"));
        KustoFormatter.Tabulate(result).Should().Contain("10");
    }

    [TestMethod]
    public async Task SimplePaging()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 1000);
        var result = (await context.RunQuery("data | project Value"));
        var pagedTable = PageOfKustoTable.Create(result.Table,10,20);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable),result.Visualization,result.QueryDuration,result.Error);
        resultPage.Get(0,0).Should().Be(10);
        resultPage.RowCount.Should().Be(20);
    }

    [TestMethod]
    public async Task PagingAcrossChunks()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = (await context.RunQuery("data | project Value"));
        var pagedTable = PageOfKustoTable.Create(result.Table, 10, 20);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable), result.Visualization, result.QueryDuration, result.Error);
        resultPage.Get(0, 0).Should().Be(10);
        resultPage.RowCount.Should().Be(20);
        resultPage.Get(0, 19).Should().Be(29);
    }

    [TestMethod]
    public async Task PagingBeyondEnd()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 1000).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = (await context.RunQuery("data | project Value"));
        var pagedTable = PageOfKustoTable.Create(result.Table, 990, 20);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable), result.Visualization, result.QueryDuration, result.Error);
        resultPage.Get(0, 0).Should().Be(990);
        resultPage.RowCount.Should().Be(10);
    }


    [TestMethod]
    public async Task ZeroLengthPage()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = (await context.RunQuery("data | project Value"));
        var pagedTable = PageOfKustoTable.Create(result.Table, 10, 0);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable), result.Visualization, result.QueryDuration, result.Error);
        resultPage.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task OutOfRangePage()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = (await context.RunQuery("data | project Value"));
        var pagedTable = PageOfKustoTable.Create(result.Table, 1000, 1000);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable), result.Visualization, result.QueryDuration, result.Error);
        resultPage.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task SlicingReassembledChunks()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();
        
        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = (await context.RunQuery("data | project Value"));
        context.MaterializeResultAsTable(result, "res");

        result = (await context.RunQuery("res | take 10"));
        result.RowCount.Should().Be(10);

    }
}
