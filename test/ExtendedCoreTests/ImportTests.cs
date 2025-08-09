using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Util;

namespace ExtendedCoreTests;

[TestClass]
public class ImportTests
{
    [TestMethod]
    public async Task NestedClassGetsFullSchema()
    {
        var child = new ChildClass { ChildName = "Test", ChildId = "123" };
        var context = new KustoQueryContext()
            .AddTable(TableBuilder.CreateFromImmutableData("test", [child]));
        var schema = await context.RunQuery("test | getschema");
        KustoFormatter.Tabulate(schema).Should().Contain("ChildName");
        KustoFormatter.Tabulate(schema).Should().Contain("ChildId");
    }

    [TestMethod]
    public async Task EnumSerialisedAsString()
    {
        var child = new EnumClass { X = TestEnum.C };
        var context = new KustoQueryContext()
            .AddTable(TableBuilder.CreateFromImmutableData("test", [child]));

        var result = await context.RunQuery("test");
        KustoFormatter.Tabulate(result).Should().Contain("X");
        KustoFormatter.Tabulate(result).Should().Contain("C");
    }

    [TestMethod]
    public async Task CustomConverter()
    {
        var converter = new KustoTypeConverter<MyId, string>(s => $"{s.Id}---{s.Number}");
        var child = new MyDto { Name = "hello", Id = new MyId("identifier", 999) };
        var context = new KustoQueryContext()
            .AddTable(TableBuilder.CreateFromImmutableData("test", [child], [converter]));
        var result = await context.RunQuery("test");
        KustoFormatter.Tabulate(result).Should().Contain("hello");
        KustoFormatter.Tabulate(result).Should().Contain("identifier---999");
    }

    [TestMethod]
    public async Task CustomConverterWithMutableData()
    {
        var converter = new KustoTypeConverter<MyId, string>(s => $"{s.Id}---{s.Number}");
        var child = new MyDto { Name = "hello", Id = new MyId("identifier", 999) };
        var context = new KustoQueryContext()
            .AddTable(TableBuilder.CreateFromVolatileData("test", [child], [converter]));
        var result = await context.RunQuery("test");
        KustoFormatter.Tabulate(result).Should().Contain("hello");
        KustoFormatter.Tabulate(result).Should().Contain("identifier---999");
    }

    [TestMethod]
    public void TypePromotion()
    {
        var data = Enumerable.Range(0, 100).Select(i => (short)i).Cast<object?>().ToArray();
        var col = ColumnHelpers.CreateFromObjectArray(data, TypeMapping.SymbolForType(typeof(short)));
        col.GetRawDataValue(50).Should().Be(50);
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

public readonly record struct MyId(string Id, int Number);

public class MyDto
{
    public string Name { get; set; } = string.Empty;
    public MyId Id { get; set; }
}

public enum TestEnum
{
    A,
    B,
    C
}
