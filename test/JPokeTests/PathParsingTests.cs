using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using JPoke;

namespace TestProject1;

[TestClass]
public class PathParsingTests
{
    [TestMethod]
    public void SimpleElementIsParsed()
    {
        var p = PathParser.Parse("abc");
        p.Length.Should().Be(1);
        p.IsTerminal.Should().BeTrue();
    }

    [TestMethod]
    public void EmptyRootElementNameIsAllowed()
    {
        var p = PathParser.Parse(".second");
        p.Length.Should().Be(2);
        p.IsTerminal.Should().BeFalse();
        p.Elements.First().Name.Should().Be("");
        p.Elements.ElementAt(1).Name.Should().Be("second");
    }

    [TestMethod]
    public void EmptyRootElementNameIsAllowedForArray()
    {
        var p = PathParser.Parse("[2].second");
        p.Length.Should().Be(2);
        p.IsTerminal.Should().BeFalse();
        p.Elements.First().Name.Should().Be("");
        p.Elements.First().IsIndex.Should().BeTrue();
        p.Elements.ElementAt(1).Name.Should().Be("second");
    }

    [TestMethod]
    public void JsonNodeIsRendered()
    {
        var obj = new[]
        {
            new { a = 1, b = 2 },
            new { a = 3, b = 4 },
        };
        var s = JsonSerializer.Serialize(obj);
        var o = JsonNode.Parse(s);
        o.Should().BeOfType<JsonArray>();
        JsonSerializer.Serialize(o).Should().Be(s);
    }
}