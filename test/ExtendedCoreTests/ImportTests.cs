using System.Collections.Immutable;
using System.Text.Json;
using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Util;
using NotNullStrings;

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
    public async Task CustomConverterWithStringArray()
    {
        var converter = new KustoTypeConverter<string[], string>(s => $"{s.Length} {s.JoinString(";")}");
        var child = new { Data = new string[] { "abc", "def" } };
        var data = Enumerable.Range(0,1).Select(_=>child).ToImmutableArray();
        var context = new KustoQueryContext()
            .WrapDataIntoTable("test",data,[converter]);
            
            //.AddTable(TableBuilder.CreateFromImmutableData("test", [child], [converter]));
        var result = await context.RunQuery("test");
        KustoFormatter.Tabulate(result).Should().Contain("2 abc;def");
   }

    
    [TestMethod]
    public async Task CustomConverterWithName()
    {
        var converter = new KustoTypeConverter<MyId, string>((name,s) => $"{name}:{s.Id}---{s.Number}");
        var child = new MyDto { Name = "hello", Id = new MyId("identifier", 999) };
        var context = new KustoQueryContext()
            .AddTable(TableBuilder.CreateFromImmutableData("test", [child], [converter]));
        var result = await context.RunQuery("test");
        KustoFormatter.Tabulate(result).Should().Contain("hello");
        KustoFormatter.Tabulate(result).Should().Contain("Id:identifier---999");
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

    [TestMethod]
    public async Task Float()
    {
        var o = new
        {
            f = (float)2,
            fn=(float?)3
        };
        var objects = new[] { o };
        var context = new KustoQueryContext();
        context.CopyDataIntoTable("data",objects );
        var results = (await context.RunQuery("data | take 1"));
        results.ToJsonString().Should().Be(JsonSerializer.Serialize(objects));

    }

    
    [TestMethod]
    public async Task Byte()
    {
       
        var o = new
        {
            f = (byte)2,
            fn=(byte?)3,
            sf = (sbyte)2,
            sfn=(sbyte?)3
        };
        
        var objects = new[] { o };
        var context = new KustoQueryContext();
        context.CopyDataIntoTable("data",objects );
        var results = (await context.RunQuery("data | take 1"));
        results.ToJsonString().Should().Be(JsonSerializer.Serialize(objects));
    }

    [TestMethod]
    public async Task ByteWrapped()
    {
       
        var o = new
        {
            f = (byte)2,
            fn=(byte?)3,
            sf = (sbyte)2,
            sfn=(sbyte?)3
        };
        
        var objects = new[] { o }.ToImmutableArray();
        var context = new KustoQueryContext();
        context.WrapDataIntoTable("data",objects );
        var results = (await context.RunQuery("data | take 1"));
        results.ToJsonString().Should().Be(JsonSerializer.Serialize(objects));
    }
    
    [TestMethod]
    public async Task Nullable()
    {
        var o = new
        {
            fn=(float?)3
        };
        var objects = new[] { o };

        var context = new KustoQueryContext();
        context.CopyDataIntoTable("data",objects );
        var results = (await context.RunQuery("data | take 1"));
        results.ToJsonString().Should()
            .Be(JsonSerializer.Serialize(objects));
    }


    [TestMethod]
    public async Task NullableWrapped()
    {
        var o = new
        {
            fn=(float?)3
        };
        var objects = new[] { o }.ToImmutableArray();
        
        var context = new KustoQueryContext();
        context.WrapDataIntoTable("data",objects );
        var results = (await context.RunQuery("data | take 1"));
        results.ToJsonString().Should()
            .Be(JsonSerializer.Serialize(objects));
    }

    
    [TestMethod]
    public async Task AllTypesSupportedForAnonymousType()
    {
        var o = new
        {
            str = "abcd",
            g = Guid.NewGuid(),
            dt = DateTime.UtcNow,
            b= (byte)1,
            sb= (sbyte)1,
            nb= (byte?)1,
            nsb= (sbyte?)1,
            i = 5,
            usht = (short)9,
            sht = (short)9,
            uln = (ulong?)10,
            lng = (long)6,
            boo = true,
            f = (float)2,
            d = (double)4,
            t = TimeSpan.FromHours(1)
        };

        var records = Enumerable.Range(0, 1).Select(i => o).ToImmutableArray();

        var context = new KustoQueryContext();
        context.CopyDataIntoTable("data", records);
        var results = (await context.RunQuery("data | take 1"));
        var r = results.ToJsonString();
        var expected = JsonSerializer.Serialize(new[] { o });
        Console.WriteLine("EXP " + expected);
        Console.WriteLine("ACT " + r);
        r.Should().Be(expected);
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
