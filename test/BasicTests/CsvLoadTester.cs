using System.Collections.Immutable;
using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using KustoLoco.FileFormats;

namespace BasicTests;

[TestClass]
public class CsvLoadTester
{
    private static KustoQueryContext CreateContext()
        => KustoQueryContext.CreateForTest();

    [TestMethod]
    public async Task TestMethod1()
    {
        var console = new SystemConsole();
        var context = CreateContext();
        var csv = """

                  Name,Count
                  acd,100
                  def,30

                  """;
        var t =CsvSerializer.Default(new KustoSettingsProvider(),console)
            .LoadFromString(csv, "data");
        t = TableBuilder.AutoInferColumnTypes(t,console);
        context.AddTable(t);
        var nameResult = (await context.RunQuery("data | where Name contains 'a'"));
        nameResult.Error.Should().BeEmpty();
        nameResult.RowCount.Should().Be(1);

        var countResult = await context.RunQuery("data | where Count > 50");
        countResult.Error.Should().BeEmpty();
        countResult.RowCount.Should().Be(1);
    }


    [TestMethod]
    public async Task Count()
    {
        var context = new KustoQueryContext();
        var rows = Enumerable.Range(0, 50000).Select(i => new Row(i.ToString(), i)).ToImmutableArray();

        context.WrapDataIntoTable("data", rows);
        var result = (await context.RunQuery("data | count"));
        KustoFormatter.Tabulate(result).Should().Contain("50000");
    }


    [TestMethod]
    public async Task Where()
    {
        var context = new KustoQueryContext();
        var rows = Enumerable.Range(0, 50000).Select(i => new Row(i.ToString(), i)).ToImmutableArray();

        context.WrapDataIntoTable("data", rows);
        var result = (await context.RunQuery("data | where Value < 10 | count"));
        KustoFormatter.Tabulate(result).Should().Contain("10");
    }

    [TestMethod]
    public void TestSkipTypeInference()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, "true");

        var csv = """

                  Name,Count
                  acd,100
                  def,30

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // When SkipTypeInference is true, Count column should remain as string
        var countColumn = table.Type.Columns.First(c => c.Name == "Count");
        var countType = TypeMapping.UnderlyingTypeForSymbol(countColumn.Type);
        countType.Should().Be(typeof(string));
    }

    [TestMethod]
    public void TestTypeInferenceEnabled()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, "false");

        var csv = """

                  Name,Count
                  acd,100
                  def,30

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // When SkipTypeInference is false (default), Count column should be inferred as long
        var countColumn = table.Type.Columns.First(c => c.Name == "Count");
        var countType = TypeMapping.UnderlyingTypeForSymbol(countColumn.Type);
        countType.Should().Be(typeof(long));
    }

    [TestMethod]
    public void TestTrimCells()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.TrimCells.Name, "true");
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, "true");

        var csv = """

                  Name,Count
                    acd  ,  100  
                    def  ,  30  

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // Values should be trimmed - check via GetColumnData
        var nameData = table.GetColumnData(0).ToArray();
        var firstValue = nameData[0] as string;
        firstValue.Should().Be("acd");
    }

    [TestMethod]
    public void TestNoTrimCells()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.TrimCells.Name, "false");
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, "true");

        var csv = """

                  Name,Count
                    acd  ,  100  
                    def  ,  30  

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // Values should NOT be trimmed - check via GetColumnData
        var nameData = table.GetColumnData(0).ToArray();
        var firstValue = nameData[0] as string;
        firstValue.Should().Be("  acd  ");
    }

    [TestMethod]
    public async Task TestSkipHeaderOnSave()
    {
        var console = new SystemConsole();
        var context = CreateContext();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipHeaderOnSave.Name, "true");

        var csv = """

                  Name,Count
                  acd,100
                  def,30

                  """;
        var serializer = CsvSerializer.Default(new KustoSettingsProvider(), console);
        var table = serializer.LoadFromString(csv, "data");
        context.AddTable(table);

        var result = await context.RunQuery("data");

        var stream = new MemoryStream();
        var saveSerializer = CsvSerializer.Default(settings, console);
        await saveSerializer.SaveTable(stream, result);

        // Read before disposing - SaveTable closes the stream
        var savedCsv = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // First line should be data, not headers
        savedCsv.Should().StartWith("acd,100");
        savedCsv.Should().NotContain("Name,Count");
    }

    [TestMethod]
    public async Task TestIncludeHeaderOnSave()
    {
        var console = new SystemConsole();
        var context = CreateContext();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipHeaderOnSave.Name, "false");

        var csv = """

                  Name,Count
                  acd,100
                  def,30

                  """;
        var serializer = CsvSerializer.Default(new KustoSettingsProvider(), console);
        var table = serializer.LoadFromString(csv, "data");
        context.AddTable(table);

        var result = await context.RunQuery("data");

        var stream = new MemoryStream();
        var saveSerializer = CsvSerializer.Default(settings, console);
        await saveSerializer.SaveTable(stream, result);

        // Read before disposing - SaveTable closes the stream
        var savedCsv = System.Text.Encoding.UTF8.GetString(stream.ToArray());

        // First line should be headers
        savedCsv.Should().StartWith("Name,Count");
    }

    [TestMethod]
    public void TestInferColumnNames()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.InferColumnNames.Name, "true");

        var csv = """

                  acd,100
                  def,30

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // When InferColumnNames is true, column names should be auto-generated
        var columnNames = table.Type.Columns.Select(c => c.Name).ToArray();
        columnNames.Should().Equal("Column0", "Column1");
        table.RowCount.Should().Be(2); // Both rows should be data
    }

    [TestMethod]
    public void TestNoInferColumnNames()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.InferColumnNames.Name, "false");

        var csv = """

                  Name,Count
                  acd,100
                  def,30

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // When InferColumnNames is false, first row should be used as headers
        var columnNames = table.Type.Columns.Select(c => c.Name).ToArray();
        columnNames.Should().Equal("Name", "Count");
        table.RowCount.Should().Be(2); // Only data rows
    }

    [TestMethod]
    public void TestCustomSeparator()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.Separator.Name, "|");
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, "true");

        var csv = """

                  Name|Count
                  acd|100
                  def|30

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // Should correctly parse pipe-separated values
        var columnNames = table.Type.Columns.Select(c => c.Name).ToArray();
        columnNames.Should().Equal("Name", "Count");
        table.RowCount.Should().Be(2);

        var nameData = table.GetColumnData(0).ToArray();
        var firstValue = nameData[0] as string;
        firstValue.Should().Be("acd");
    }

    [TestMethod]
    public void TestAutoDetectSeparator()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.Separator.Name, "");

        var csv = """

                  Name;Count
                  acd;100
                  def;30

                  """;
        var serializer = CsvSerializer.Default(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // Should auto-detect semicolon separator
        var columnNames = table.Type.Columns.Select(c => c.Name).ToArray();
        columnNames.Should().Equal("Name", "Count");
        table.RowCount.Should().Be(2);
    }

    [TestMethod]
    public void TestTsvSeparator()
    {
        var console = new SystemConsole();
        var settings = new KustoSettingsProvider();
        settings.Set(CsvSerializer.CsvSerializerSettings.SkipTypeInference.Name, "true");

        var csv = "Name\tCount\nacd\t100\ndef\t30";
        var serializer = CsvSerializer.Tsv(settings, console);
        var table = serializer.LoadFromString(csv, "data");

        // Should correctly parse tab-separated values
        var columnNames = table.Type.Columns.Select(c => c.Name).ToArray();
        columnNames.Should().Equal("Name", "Count");
        table.RowCount.Should().Be(2);

        var nameData = table.GetColumnData(0).ToArray();
        var firstValue = nameData[0] as string;
        firstValue.Should().Be("acd");
    }
}

public readonly record struct Row(string Name, int Value);
