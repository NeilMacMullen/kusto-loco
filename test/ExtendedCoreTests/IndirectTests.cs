using System.Collections.Immutable;
using BabyKusto.Core;
using BabyKusto.Core.Util;
using FluentAssertions;
using Kusto.Language.Symbols;

namespace ExtendedCoreTests;

[TestClass]
public class IndirectTests
{
    private MappedColumn<string> MakeDecimatedColumn(int sourceCount)
    {
        var originalData = Enumerable.Range(0, sourceCount)
            .Select(i => i.ToString()).ToArray();
        var cs = new InMemoryColumn<string>(ScalarTypes.String, originalData);
        var backing = MappedColumn<string>.Create(
            Enumerable.Range(0, sourceCount).Where(i => i % 10 == 0).ToImmutableArray()
            , cs);
        return (MappedColumn<string>)backing;
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
        var backing = (TypedBaseColumn<string>)MakeDecimatedColumn(100);
        backing.RowCount.Should().Be(10);
        backing[0].Should().Be("0");
        backing[1].Should().Be("10");
        backing.GetRawDataValue(0).Should().Be("0");
    }


    [TestMethod]
    public void SlicingIndirectedColumnCanWork()
    {
        var backing = (TypedBaseColumn<string>)MakeDecimatedColumn(100);
        var sliced = backing.Slice(5, 2);
        sliced.RowCount.Should().Be(2);
        sliced.GetRawDataValue(0).Should().Be("50");
    }


    [Ignore("decided not to implement this!")]
    [TestMethod]
    public void MappedIndirectionFlattens()
    {
        var backing = MakeDecimatedColumn(100);
        var flattened = ColumnHelpers.MapColumn(backing, new[] { 0, 1 }.ToImmutableArray()) as MappedColumn<string>;
        flattened!.RowCount.Should().Be(2);
        flattened!.IndirectIndex(0).Should().Be(backing.IndirectIndex(0));
        flattened!.IndirectIndex(1).Should().Be(backing.IndirectIndex(1));
    }

    [TestMethod]
    public void IndirectingSingleValueColumnReturnsSingleValue()
    {
        var sv = new SingleValueColumn<string>(ScalarTypes.String, "HELLO", 100);
        var second = ColumnHelpers.MapColumn(sv, new[] { 0, 1 }.ToImmutableArray());
        second.GetType().Should().Be(typeof(SingleValueColumn<string>));
        second.RowCount.Should().Be(2);
        second.GetRawDataValue(0).Should().Be("HELLO");
    }
}