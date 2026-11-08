using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class ProjectConstantTests : TestMethods
{
    [TestMethod]
    public async Task ConstantsOverNoRowsGiveNoRows()
    {
        var result = await Result("datatable(a:long)[1] | where a == 0 | project x = 'none'");
        result.RowCount.Should().Be(0);
    }

    [TestMethod]
    public async Task ConstantsOverNoRowsAddNothingToAUnion()
    {
        var result = await ResultAsString(
            "union (datatable(a:long)[1] | where a == 0 | project x = 'none'), (print x = 'y')");
        result.Should().Be("y");
    }

    [TestMethod]
    public async Task ConstantsGiveOneRowPerInputRow()
    {
        var result = await ResultAsString("datatable(a:long)[1, 2, 3] | project x = 'c', y = strcat('a', 'b')");
        result.Should().Be("c,ab,c,ab,c,ab");
    }
}
