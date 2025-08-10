using AwesomeAssertions;
using KustoLoco.Core.DataSource;
using KustoLoco.Core.DataSource.Columns;

namespace ExtendedCoreTests;

[TestClass]
public class SingleValueTests
{
    [TestMethod]
    public void SingleWorks()
    {
        var backing = new GenericSingleValueColumnOfstring("hello", 10);
        backing.RowCount.Should().Be(10);

        backing[0].Should().Be("hello");
        backing[9].Should().Be("hello");
        backing.GetRawDataValue(0).Should().Be("hello");
    }

    [TestMethod]
    public void BuilderCreatesSingleValueColumn()
    {
        var dataset = NullableSetOfint.FromObjectsOfCorrectType([(int) 1]);
        var column = GenericColumnFactoryOfint.CreateFromDataSet(dataset);
        column.Should().BeOfType<GenericSingleValueColumnOfint>();
    }

    [TestMethod]
    public void BuilderCreatesSingleValueColumnAfterExpansion()
    {
        var column = new GenericInMemoryColumnOfint([99]);
        var expanded = ColumnFactory.Inflate(column, 100);
        expanded.Should().BeOfType<GenericSingleValueColumnOfint>();
        expanded.RowCount.Should().Be(100);
        expanded.GetRawDataValue(7).Should().Be(99);
    }
}
