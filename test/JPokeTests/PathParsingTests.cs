using FluentAssertions;

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
}