using FluentAssertions;
using KustoSupport;
using LogSetup;
using NLog;

namespace BasicTests;

[TestClass]
public class OperatorTests
{
    public OperatorTests()
    {
        LoggingExtensions.SetupLoggingForTest(LogLevel.Trace);
    }

    private static KustoQueryContext CreateContext() => KustoQueryContext.WithFullDebug();

    [TestMethod]
    public async Task GetSchema()
    {
        var context = CreateContext();
        var rows = Enumerable.Range(0, 20).Select(i => new Row(i.ToString(), i)).ToArray();

        context.AddTableFromRecords("data", rows);
        var result = (await context.RunQuery("data | getschema"));
        var schema = KustoFormatter.Tabulate(result.Results);
        Console.WriteLine(schema);
        schema.Should().Contain(nameof(Row.Name));
        schema.Should().Contain(nameof(Row.Value));
    }
}