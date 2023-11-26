using CsvSupport;
using FluentAssertions;
using KustoSupport;

namespace BasicTests
{
    [TestClass]
    public class CsvLoadTester
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var context = new KustoQueryContext();
            var csv = @"
Name,Count
acd,100
def,30
";
            CsvLoader.LoadFromString(csv, "data", context);
            var nameResult = (await context.RunQuery("data | where Name contains 'a'"));
            nameResult.Error.Should().BeEmpty();
            nameResult.Results.Count.Should().Be(1);

            var countResult = await context.RunQuery("data | where Count > 50");
            countResult.Error.Should().BeEmpty();
            countResult.Results.Count.Should().Be(1);
        }
    }
}