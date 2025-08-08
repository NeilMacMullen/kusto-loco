using AwesomeAssertions;
using KustoLoco.Core;

namespace ExtendedCoreTests;

[TestClass]
public class ImportTests
{
    [TestMethod]
    public async Task NestedClassGetsFullSchema()
    {
        var child = new ChildClass { ChildName = "Test", ChildId = "123" };
        var table = TableBuilder.CreateFromImmutableData("test", [child]);
        var context = new KustoQueryContext();
        context.AddTable(table);
        var schema = await context.RunQuery("test | getschema");
        KustoFormatter.Tabulate(schema).Should().Contain("ChildName");
        KustoFormatter.Tabulate(schema).Should().Contain("ChildId");
    }

    [TestMethod]
    public async Task EnumSerialisedAsString()
    {
        var child = new EnumClass() { X=TestEnum.C };
        var table = TableBuilder.CreateFromImmutableData("test", [child]);
        var context = new KustoQueryContext();
        context.AddTable(table);
        var result = await context.RunQuery("test");
        KustoFormatter.Tabulate(result).Should().Contain("X");
        KustoFormatter.Tabulate(result).Should().Contain("C");
    }
}

public class EnumClass
{
    public TestEnum X { get; set; } = TestEnum.A;
}
public class BaseClass
{
    public string ChildId { get; set; } = string.Empty;
}

public class ChildClass : BaseClass
{
    public string ChildName { get; set; } = string.Empty;
}

public enum TestEnum
{
    A,
    B,
    C
}
