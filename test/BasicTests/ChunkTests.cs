using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.Util;
using LogSetup;
using NLog;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class ChunkTests
{
    public ChunkTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    private static KustoQueryContext CreateContext() => KustoQueryContext.CreateForTest();

    private void AddChunkedTableFromRecords<T>(KustoQueryContext context, string tableName,
        IReadOnlyCollection<T> records, int chunkSize)
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

        AddChunkedTableFromRecords(context, "data", rows, 2);
        var result = await context.RunQuery("data | take 5");
        result.RowCount.Should().Be(5);
        //TODO - here compare tabulated output for more coverage
    }


    [TestMethod]
    public async Task Count()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 2);
        var result = await context.RunQuery("data | count");
        KustoFormatter.Tabulate(result).Should().Contain("20");
    }

    [TestMethod]
    public async Task WhereCount()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 2);
        var result = await context.RunQuery("data | where Value < 10 | count");
        KustoFormatter.Tabulate(result).Should().Contain("10");
    }

    [TestMethod]
    public async Task SimplePaging()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 1000);
        var result = await context.RunQuery("data | project Value");
        var pagedTable = PageOfKustoTable.Create(result.Table, 10, 20);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable),
            result.Visualization, result.QueryDuration, result.Error);
        resultPage.Get(0, 0).Should().Be(10);
        resultPage.RowCount.Should().Be(20);
    }

    [TestMethod]
    public async Task PagingAcrossChunks()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = await context.RunQuery("data | project Value");
        var pagedTable = PageOfKustoTable.Create(result.Table, 10, 20);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable),
            result.Visualization, result.QueryDuration, result.Error);
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
        var result = await context.RunQuery("data | project Value");
        var pagedTable = PageOfKustoTable.Create(result.Table, 990, 20);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable),
            result.Visualization, result.QueryDuration, result.Error);
        resultPage.Get(0, 0).Should().Be(990);
        resultPage.RowCount.Should().Be(10);
    }


    [TestMethod]
    public async Task ZeroLengthPage()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = await context.RunQuery("data | project Value");
        var pagedTable = PageOfKustoTable.Create(result.Table, 10, 0);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable),
            result.Visualization, result.QueryDuration, result.Error);
        resultPage.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task OutOfRangePage()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = await context.RunQuery("data | project Value");
        var pagedTable = PageOfKustoTable.Create(result.Table, 1000, 1000);
        var resultPage = new KustoQueryResult(result.Query, InMemoryTableSource.FromITableSource(pagedTable),
            result.Visualization, result.QueryDuration, result.Error);
        resultPage.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task SlicingReassembledChunks()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 3);
        var result = await context.RunQuery("data | project Value");
        context.MaterializeResultAsTable(result, "res");

        result = await context.RunQuery("res | take 10");
        result.RowCount.Should().Be(10);
    }

    [TestMethod]
    public async Task SimplestUnion()
    {
        //create two single-row tables and use union to join them
        var context = CreateContext();
        var rows = Enumerable.Range(0, 2).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "table1", [rows[0]], 100);
        AddChunkedTableFromRecords(context, "table2", [rows[1]], 100);
        var result = await context.RunQuery("table1 | union table2 | summarize count() by Name | project count_");
        // Since the ids are unique, we shoudl have two rows each of count 1
        result.RowCount.Should().Be(2);
        foreach (var row in result.EnumerateRows()) row[0].Should().Be(1);
    }

    [TestMethod]
    public async Task SimpleUnionWithChunkSpanningTables()
    {
        //create a couple of multi-chunk tables that we're going to combine with the
        //union operator
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "table1", rows, 3);
        AddChunkedTableFromRecords(context, "table2", rows, 5);
        var result = await context.RunQuery("table1 | union table2");
        result.RowCount.Should().Be(rows.Length * 2);

        result = await context.RunQuery("table1 | union table2 | count");
        result.Get(0, 0).Should().Be(rows.Length * 2);

        result = await context.RunQuery("table1 | union table2 | summarize count()");
        result.Get(0, 0).Should().Be(rows.Length * 2);

        result = await context.RunQuery("table1 | union table2 | summarize count() by Name | project count_");
        //we've used the same set of ids in each table so the count for each row should be 2.
        foreach (var row in result.EnumerateRows()) row[0].Should().Be(2);
    }


    [TestMethod]
    public async Task RowNumber()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        AddChunkedTableFromRecords(context, "data", rows, 10);
        var result = await context.RunQuery(
            """
            data
            | serialize
            | extend r = row_number(0)
            | extend same=Value==r
            | where same
            """);
        result.RowCount.Should().Be(20);
        //TODO - here compare tabulated output for more coverage
    }

    [TestMethod]
    public async Task LookupAcrossChunks()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 100).Select(i => new Row(i.ToString(), i)).ToArray();
        var lookupRows = Enumerable.Range(95, 10).Select(i => new Row(i.ToString(), 95))
            .ToArray();
        AddChunkedTableFromRecords(context, "data", rows, 3);
        AddChunkedTableFromRecords(context, "lk", lookupRows, 3);

        var result = await context.RunQuery("""
                                            data 
                                            | lookup kind =inner lk on Value
                                            | project Name1
                                            """
                                            );
        result.RowCount.Should().Be(10);

        var col = result.ColumnDefinitions()[0];
        var str= result.EnumerateColumnData(col)
            .Select(i=>int.Parse((string)i!))
            .Order()
            .JoinString(",");
        str.Should().Be("95,96,97,98,99,100,101,102,103,104");
    }

}
