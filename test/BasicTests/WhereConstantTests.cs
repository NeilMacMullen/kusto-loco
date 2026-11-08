using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class WhereConstantTests : TestMethods
{
    [TestMethod]
    public async Task FalseGivesAnEmptyTableWithTheColumns()
    {
        var result = await Result("datatable(a:long)[1] | where false");
        result.Error.Should().BeEmpty();
        result.RowCount.Should().Be(0);
        result.ColumnNames().Should().Equal("a");
    }

    [TestMethod]
    public async Task FalseThenProjectGivesNoRows()
    {
        var result = await Result("datatable(a:long)[1] | where false | project a");
        result.Error.Should().BeEmpty();
        result.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task FalseThenCountIsZero()
    {
        var result = await ResultAsString("datatable(a:long)[1] | where false | count");
        result.Should().Be("0");
    }

    [TestMethod]
    public async Task TrueKeepsEveryRow()
    {
        var result = await ResultAsString("datatable(a:long)[1, 2] | where true");
        result.Should().Be("1,2");
    }
}
