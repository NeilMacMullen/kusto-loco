using System.Collections.Immutable;
using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Console;
using KustoLoco.Core.Settings;
using KustoLoco.FileFormats;

namespace BasicTests
{
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
            var csv = @"
Name,Count
acd,100
def,30
";
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
    }

    public readonly record struct Row(string Name, int Value);
}
