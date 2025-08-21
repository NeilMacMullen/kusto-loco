using System.Collections.Immutable;
using AwesomeAssertions;
using KustoLoco.Core;

namespace BasicTests;

[TestClass]
public class ContextOperationTests : TestMethods
{
    private KustoQueryContext GetContext()
    {

        var context = CreateContext();
        var data = Enumerable.Range(0, 100).Select(i => new { N = i })
            .ToImmutableArray();
        context.WrapDataIntoTable("data", data);
        return context;
    }

    [TestMethod]
    public async Task Share()
    {
        var context = GetContext();
        context.ShareTable("data","new");
        var result = await context.RunQuery("new | take 1");
        result.Error.Should().Be("");
        result = await context.RunQuery("data | take 1");
        result.Error.Should().Be("");
    }

    [TestMethod]
    public async Task Rename()
    {
        var context = GetContext();
        context.RenameTable("data", "new");
        var result = await context.RunQuery("new | take 1");
        result.Error.Should().Be("");
        result = await context.RunQuery("data | take 1");
        result.Error.Should().Contain("does not");
    }

    [TestMethod]
    public async Task SelfRename()
    {
        var context = GetContext();
        context.RenameTable("data", "data");
        var result = await context.RunQuery("data | take 1");
        result.Error.Should().Be("");
    }

    [TestMethod]
    public async Task Remove()
    {
        var context = GetContext();
        context.RemoveTable("data");
        var result = await context.RunQuery("data | take 1");
        result.Error.Should().Contain("does not");
    }
}
