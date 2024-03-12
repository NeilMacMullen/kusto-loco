using KustoLoco.Core;
using FluentAssertions;
using KustoLoco.Core.DataSource;

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

    [TestMethod]
    public void BuilderCreatesSingleValueColumn()
    {
        var column = ColumnFactory.Create(new int?[1]);
        column.Should().BeOfType<SingleValueColumn<int?>>();
    }

    [TestMethod]
    public void BuilderCreatesSingleValueColumnAfterExpansion()
    {
        var column = new InMemoryColumn<int?>([99]);
        var expanded = ColumnFactory.Inflate(column, 100);
        expanded.Should().BeOfType<SingleValueColumn<int?>>();
        expanded.RowCount.Should().Be(100);
        expanded.GetRawDataValue(7).Should().Be(99);
    }
}