using System.Collections.Specialized;
using FluentAssertions;
using KustoLoco.Core;
using KustoLoco.Core.Util;
// ReSharper disable StringLiteralTypo

// ReSharper disable UnusedVariable

namespace BasicTests;

[TestClass]
public class TableBuilderTests
{
    [TestMethod]
    public void TypeMappingTests()
    {
        var a1 = TypeMapping.CastOrConvertToNullable<int?>((short)5);
        var b1 = TypeMapping.CastOrConvertToNullable<int?>((short?)5);
        var c1 = TypeMapping.CastOrConvertToNullable<int?>(5);
        var d1 = TypeMapping.CastOrConvertToNullable<int?>((int?)5);
        var e1 = TypeMapping.CastOrConvertToNullable<int?>(null);
        var f1 = TypeMapping.CastOrConvertToNullable<int?>(null);
        var g1 = TypeMapping.CastOrConvertToNullable<string?>("abc");
        var g2 = TypeMapping.CastOrConvertToNullable<string?>(null);
    }

    [TestMethod]
    public void AllStandardTypesCanBeSupported()
    {
        var o = new OrderedDictionary
        {
            ["int"] = 1,
            ["nint"] = (int?)1,

            ["short"] = (short)1,
            ["nshort"] = (short?)1,
            ["ushort"] = (ushort)1,

            ["slong"] = (long)1,
            ["nlong"] = (long?)1,

            ["float"] = (float)1,
            ["nfloat"] = (float?)1,

            ["guid"] = Guid.NewGuid(),
            ["nguid"] = (Guid?)Guid.NewGuid(),
        };
        var builder = TableBuilder.FromOrderedDictionarySet("test", [o]);
    }
    [TestMethod]
    public void ColumnNamesAreUnique()
    {
        var data =new[] { 1, 2, 3 };
        var builder =

            TableBuilder.CreateEmpty("test", data.Length)
                .WithColumn("A", data)
            .WithColumn("", data)
            .WithColumn("A", data)
                .WithColumn("", data)

            ;
        var src = builder.ToTableSource();
        src.ColumnNames.Should()
            .BeEquivalentTo("A", "_column", "A_1", "_column_1");

    }
}
