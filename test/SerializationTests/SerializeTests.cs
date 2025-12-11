using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;
using System;
using System.Text;

namespace SerializationTests;

[TestClass]
public class SerializeTests
{
    private readonly ITableSerializer _csvSerializer;
    private readonly KustoResultSerializer _kustoResultSerializer;
    private readonly ParquetSerializer _parquetSerializer;

    public SerializeTests()
    {
        _parquetSerializer = new ParquetSerializer(new KustoSettingsProvider(), new SystemConsole());
        _csvSerializer = CsvSerializer.Default(new KustoSettingsProvider(), new SystemConsole());
        _kustoResultSerializer = ParquetResultSerializer.Default;
    }

    [TestMethod]
    public async Task SerialisingFailedQueryResultToParquetDoesNotThrow()
    {
        using var stream = new MemoryStream();
        await _parquetSerializer.SaveTable(stream, KustoQueryResult.Empty);
        stream.ToArray().Length.Should().Be(0);
    }

    [TestMethod]
    public async Task SerialisingFailedQueryResultToCsvDoesNotThrow()
    {
        var sw = new MemoryStream();
        await _csvSerializer.SaveTable(sw, KustoQueryResult.Empty);
        var retStr = Encoding.UTF8.GetString(sw.ToArray());
        retStr.Should().Be(string.Empty);
    }

    [TestMethod]
    public async Task NonNullableCanBeRead()
    {
        var count = 100;
        var strm = new MemoryStream();
        await _parquetSerializer.SaveTestTable(strm,count);
        var bytes =strm.ToArray();
        
        var lr = await _kustoResultSerializer.FromBytes(bytes);
        lr.RowCount.Should().Be(count);
        lr.ColumnCount.Should().Be(1);
        lr.Get(0, count-1).Should().Be(count - 1);
    }

    [TestMethod]
    public async Task SerialiseEmptyResult()
    {
        var result = await CreateEmptyResultWithColumn();
        var bytes = await _kustoResultSerializer.GetBytes(result);
        bytes.Length.Should().NotBe(0);

        var lr = await _kustoResultSerializer.FromBytes(bytes);
        lr.RowCount.Should().Be(0);
        lr.ColumnCount.Should().Be(0);
    }


    [TestMethod]
    public async Task EmptyResult()
    {   
            var result = await _kustoResultSerializer.Serialize(KustoQueryResult.Empty);
            var r = await _kustoResultSerializer.Deserialize(result);
            r.ColumnCount.Should().Be(0);
            r.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task SerialiseNonEmptyResult()
    {
        var result = await CreateSimpleResult();
        var dto = await _kustoResultSerializer.Serialize(result);
        var roundTripped = await _kustoResultSerializer.Deserialize(dto);
        CheckResultEquivalency(roundTripped, result);
    }


    [TestMethod]
    public async Task SerializeError()
    {
        var result = await Query("errrror");
        var dto = await _kustoResultSerializer.Serialize(result);
        var roundTripped = await _kustoResultSerializer.Deserialize(dto);
        CheckResultEquivalency(roundTripped, result);
    }


    [TestMethod]
    public async Task SerialiseChart()
    {
        var result = await Query("let T = datatable(A:int)[1,2,3,]; T| project X=A,Y=A | render linechart");
        result.Visualization.ChartType.Should().Be("linechart");
        var dto = await _kustoResultSerializer.Serialize(result);
        var roundTripped = await _kustoResultSerializer.Deserialize(dto);
        CheckResultEquivalency(roundTripped, result);
    }

    [TestMethod]
    public async Task SerialiseLargeTimeSpan()
    {
        var result = await Query("let X = datetime(1 dec 2025) - datetime(1 jan 1970);toscalar(X)");
        var dto = await _kustoResultSerializer.Serialize(result);
        var roundTripped = await _kustoResultSerializer.Deserialize(dto);
        CheckResultEquivalency(roundTripped, result);
    }
    [TestMethod]
    public async Task SerialiseNull()
    {
        var result = await Query("print int(null)");
        var dto = await _kustoResultSerializer.Serialize(result);
        var roundTripped = await _kustoResultSerializer.Deserialize(dto);
        CheckResultEquivalency(roundTripped, result);
    }


    private void CheckResultEquivalency(KustoQueryResult roundTripped, KustoQueryResult expected)
    {
        roundTripped.ColumnCount.Should().Be(expected.ColumnCount);
        roundTripped.RowCount.Should().Be(expected.RowCount);
        roundTripped.Query.Should().Be(expected.Query);
        roundTripped.Error.Should().Be(expected.Error);
        roundTripped.QueryDuration.Should().BeCloseTo(expected.QueryDuration, TimeSpan.FromMilliseconds(1));
        roundTripped.Visualization.Should().Be(expected.Visualization);
        roundTripped.ColumnDefinitions().Should().BeEquivalentTo(expected.ColumnDefinitions());
        foreach (var row in Enumerable.Range(0, expected.RowCount))
        {
            foreach(var col in Enumerable.Range(0, expected.ColumnCount))
            {
                var expectedValue = expected.Get(col, row);
                var actualValue = roundTripped.Get(col, row);
                actualValue.Should().BeEquivalentTo(expectedValue,
                    $"because result value at row:{row} col:{col} should be same");
            }
           
        }
    }

    private async Task<KustoQueryResult> Query(string query)
    {
        var context = new KustoQueryContext();
        return await context.RunQuery(query);
    }

    private async Task<KustoQueryResult> CreateEmptyResultWithColumn()
    {
        var query = "let T = datatable(A:int)[1,2,3,]; T | where A > 10";
        var result = await Query(query);
        result.ColumnCount.Should().Be(1);
        result.RowCount.Should().Be(0);
        return result;
    }

    private async Task<KustoQueryResult> CreateSimpleResult()
    {
        return await Query("let T = datatable(A:int)[1,2,3,]; T");
    }
}
