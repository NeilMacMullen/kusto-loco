using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class MvExpandTests : TestMethods
{
    [TestMethod]
    public async Task SimpleMvExpand()
    {
        var query = """
                    datatable(id: long, values: dynamic)
                        [
                            1, dynamic([10, 20, 30])
                        ]
                        | mv-expand values
                    """;

        var result = await ResultAsLines(query);
        result.Should().Be(
            """
            1,10
            1,20
            1,30
            """);
    }

   

    [TestMethod]
    public async Task MvExpand_MixedArrayTypes()
    {
        // Arrange - From Microsoft docs: Single column - array expansion with mixed types
        var query = """
                    datatable (a: int, b: dynamic)
                    [
                        1, dynamic([10, 20]),
                        2, dynamic(["a", "b"])
                    ]
                    | mv-expand b
                    """;

        var expected = """
                       1,10
                       1,20
                       2,a
                       2,b
                       """;

        var result = await ResultAsLines(query);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task MvExpand_BagExpansion()
    {
        // Arrange - From Microsoft docs: Single column - bag expansion
        var query = """
                    datatable (a: int, b: dynamic)
                    [
                        1, dynamic({"prop1": "a1", "prop2": "b1"}),
                        2, dynamic({"prop1": "a2", "prop2": "b2"})
                    ]
                    | mv-expand b
                    """;

        var expected = Squash("""
                       1,{"prop1":"a1"}
                       1,{"prop2":"b1"}
                       2,{"prop1":"a2"}
                       2,{"prop2":"b2"}
                       """);

        var result = await ResultAsLines(query);
        Squash(result).Should().Be(expected);
    }


    [TestMethod]
    [Ignore]
    public async Task MvExpand_WithItemIndex()
    {
        // Arrange - From Microsoft docs: Using with_itemindex
        var query =
            """
            range x from 1 to 4 step 1
            | summarize x = make_list(x)
            | mv-expand with_itemindex=Index x
            """;

        var expected =
            """
            1,0
            2,1
            3,2
            4,3
            """;

        var result = await ResultAsLines(query);
        result.Should().Be(expected);
    }
}
