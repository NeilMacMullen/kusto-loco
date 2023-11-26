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
}