using FluentAssertions;
using KustoLoco.ScottPlotRendering;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RenderTests;

[TestClass]
public sealed class AxisTests
{
    [TestMethod]
    public void NumericAxisReturnsExpectedValues()
    {
        var axis = new NumericAxisLookup<long>();

        axis.AxisValueFor(null).Should().Be(0);
        axis.AxisValueFor((long)123).Should().Be(123.0);
    }

    [TestMethod]
    public void StringAxisReturnsExpectedValues()
    {
        object?[] strings = "abc def ghi jkl mno pqr stu vwxyz".Split(' ')
            .ToArray();
        var axis = new StringAxisLookup(strings);

        axis.AxisValueFor(null).Should().Be(0);
        axis.AxisValueFor("abc").Should().Be(1.0);
        axis.AxisValueFor("def").Should().Be(2.0);
    }

    [TestMethod]
    public void BoolAxisReturnsExpectedValues()
    {
        var axis = new BoolAxisLookup();

        axis.AxisValueFor(null).Should().Be(0);
        axis.AxisValueFor(false).Should().Be(1.0);
        axis.AxisValueFor(true).Should().Be(2.0);
    }

    [TestMethod]
    public void GuidAxisReturnsExpectedValues()
    {
        var guids = new[]
        {
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        };
        var axis = new GuidAxisLookup(guids.Cast<object?>().ToArray());

        axis.AxisValueFor(null).Should().Be(0);
        axis.AxisValueFor(guids[0]).Should().Be(1);
        axis.AxisValueFor(guids[3]).Should().Be(4);
    }
}
