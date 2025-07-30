using AwesomeAssertions;
using KustoLoco.Core.DataSource.Columns;
using KustoLoco.Core.Util;

namespace ExtendedCoreTests;

[TestClass]
public class IndirectTests
{
    private GenericMappedColumnOfstring MakeDecimatedColumn(int sourceCount)
    {
        var originalData = Enumerable.Range(0, sourceCount)
            .Select(i => i.ToString()).Cast<object?>().ToArray();
        var cs = new GenericInMemoryColumnOfstring(originalData);
        var backing = GenericMappedColumnOfstring.Create(
            [..Enumerable.Range(0, sourceCount).Where(i => i % 10 == 0)]
            , cs);
        return (GenericMappedColumnOfstring)backing;
    }

    [TestMethod]
    public void IndirectionWorks()
    {
        var backing = MakeDecimatedColumn(100);
        backing.RowCount.Should().Be(10);

        backing[0].Should().Be("0");
        backing[1].Should().Be("10");
        backing.GetRawDataValue(0).Should().Be("0");
    }

    [TestMethod]
    public void IndirectionFollowedViaCasting()
    {
        var backing = (GenericTypedBaseColumnOfstring)MakeDecimatedColumn(100);
        backing.RowCount.Should().Be(10);
        backing[0].Should().Be("0");
        backing[1].Should().Be("10");
        backing.GetRawDataValue(0).Should().Be("0");
    }


    [TestMethod]
    public void SlicingIndirectedColumnCanWork()
    {
        var backing = (GenericTypedBaseColumnOfstring)MakeDecimatedColumn(100);
        var sliced = backing.Slice(5, 2);
        sliced.RowCount.Should().Be(2);
        sliced.GetRawDataValue(0).Should().Be("50");
    }


    [Ignore("decided not to implement this!")]
    [TestMethod]
    public void MappedIndirectionFlattens()
    {
        var backing = MakeDecimatedColumn(100);
        var flattened = ColumnHelpers.MapColumn(backing, [0, 1]) as MappedColumn<string>;
        flattened!.RowCount.Should().Be(2);
        flattened.IndirectIndex(0).Should().Be(backing.IndirectIndex(0));
        flattened.IndirectIndex(1).Should().Be(backing.IndirectIndex(1));
    }

    [TestMethod]
    public void IndirectingSingleValueColumnReturnsSingleValue()
    {
        var sv = new SingleValueColumn<string>("HELLO", 100);
        var second = ColumnHelpers.MapColumn(sv, [0, 1]);
        second.GetType().Should().Be(typeof(SingleValueColumn<string>));
        second.RowCount.Should().Be(2);
        second.GetRawDataValue(0).Should().Be("HELLO");
    }

    [TestMethod]
    public void SlicingSingleValueColumnReturnsSingleValue()
    {
        var sv = new SingleValueColumn<string>("HELLO", 100);
        var sliced = sv.Slice(10, 20);
        sliced.Should().BeOfType<SingleValueColumn<string>>();
        sliced.RowCount.Should().Be(20);
        sliced.GetRawDataValue(0).Should().Be("HELLO");
    }
    [TestMethod] public void  InMemoryCanBeCast()
    {
        var data = new int[]{1, 2, 3};
        var col = new GenericInMemoryColumnOfint(data.Cast<object?>().ToArray());
        var baseCol = (GenericTypedBaseColumnOfint)col;
        
    }

}
