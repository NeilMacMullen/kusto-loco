using AwesomeAssertions;
using Parquet;
using Parquet.Data;
using Parquet.Schema;

namespace SerializationTests;

[TestClass]
public class RawParquetTests
{
    private async Task<byte[]> WriteTimespans(TimeSpan timespan)
    {
        var stream = new MemoryStream();
        var field = new TimeSpanDataField("Duration",TimeSpanFormat.MicroSeconds) ;

        var schema = new ParquetSchema(new Field[]{field});
        await using (var writer = await ParquetWriter.CreateAsync(schema, stream))
        {
            using var groupWriter = writer.CreateRowGroup();
            var dataColumn = new DataColumn(field, new [] {timespan});
            await groupWriter.WriteColumnAsync(dataColumn);
        }
        return stream.ToArray();
    }

    private async Task<TimeSpan?> ReadTimespan(byte[] bytes)
    {
        var fileStream = new MemoryStream(bytes);
        using var fileReader = await ParquetReader.CreateAsync(fileStream);
        var rowGroup = await fileReader.ReadEntireRowGroupAsync();
        var column = rowGroup[0];
        return column.Data.GetValue(0) as TimeSpan?;
    }

    private async Task CheckTimeSpan(TimeSpan ts)
    {
        var bytes = await WriteTimespans(ts);
        var ret = await  ReadTimespan(bytes);
        ret.Should().Be(ts);
    }

    [TestMethod]
    public async Task TestTimespan()
    {
        var longTs = DateTime.Now - DateTime.UnixEpoch;
        await CheckTimeSpan(TimeSpan.FromDays(23));
        await CheckTimeSpan(TimeSpan.FromDays(60));

    }
}
