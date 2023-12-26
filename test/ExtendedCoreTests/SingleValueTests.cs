using BabyKusto.Core;
using FluentAssertions;

namespace ExtendedCoreTests;

[TestClass]
public class SingleValueTests
{
    [TestMethod]
    public void SingleWorks()
    {
        var backing = new SingleValueColumn<string>("hello", 10);
        backing.RowCount.Should().Be(10);

        backing[0].Should().Be("hello");
        backing[9].Should().Be("hello");
        backing.GetRawDataValue(0).Should().Be("hello");
    }
}