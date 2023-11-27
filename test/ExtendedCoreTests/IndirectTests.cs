using BabyKusto.Core;
using FluentAssertions;
using Kusto.Language.Symbols;

namespace ExtendedCoreTests;

[TestClass]
public class IndirectTests
{
    private IndirectColumn<string> MakeDecimatedColumn(int sourceCount)
    {
        var originalData = Enumerable.Range(0, sourceCount)
            .Select(i => i.ToString()).ToArray();
        var cs = new Column<string>(ScalarTypes.String, originalData);
        var backing = new IndirectColumn<string>(
            Enumerable.Range(0, sourceCount).Where(i => i % 10 == 0).ToArray()
            , cs);
        return backing;
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
        var backing = (Column<string>)MakeDecimatedColumn(100);
        backing.RowCount.Should().Be(10);
        backing[0].Should().Be("0");
        backing[1].Should().Be("10");
        backing.GetRawDataValue(0).Should().Be("0");
    }


    [TestMethod]
    public void SlicingIndirectedColumnCanWork()
    {
        var backing = (Column<string>)MakeDecimatedColumn(100);
        var sliced = backing.Slice(5, 2);
        sliced.RowCount.Should().Be(2);
        sliced.GetRawDataValue(0).Should().Be("50");
    }

    [TestMethod]
    public void PassThruIndirectionReturnsOriginalColumn()
    {
        var backing = MakeDecimatedColumn(100);
        var builder = backing.CreateIndirectBuilder(IndirectPolicy.Passthru);
        var same = builder.CreateIndirectColumn(Array.Empty<int>());
        same.Should().BeSameAs(backing);
    }

    [Ignore("decided not to implement this!")]
    [TestMethod]
    public void MappedIndirectionFlattens()
    {
        var backing = MakeDecimatedColumn(100);
        var builder = backing.CreateIndirectBuilder(IndirectPolicy.Map);
        var flattened = builder.CreateIndirectColumn(new[] { 0, 1 }) as IndirectColumn<string>;
        flattened!.RowCount.Should().Be(2);
        flattened!.IndirectIndex(0).Should().Be(backing.IndirectIndex(0));
        flattened!.IndirectIndex(1).Should().Be(backing.IndirectIndex(1));
    }

    [TestMethod]
    public void IndirectingSingleValueColumnReturnsSingleValue()
    {
        var sv = new SingleValueColumn<string>(ScalarTypes.String, "HELLO", 100);
        var builder = sv.CreateIndirectBuilder(IndirectPolicy.Map);
        var second = builder.CreateIndirectColumn(new[] { 0, 1 });
        second.GetType().Should().Be(typeof(SingleValueColumn<string>));
        second.RowCount.Should().Be(2);
        second.GetRawDataValue(0).Should().Be("HELLO");
    }
}