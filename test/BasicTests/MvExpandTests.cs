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

    [TestMethod]
    public async Task MvExpand_MultipleColumns()
    {
        // Expand multiple columns simultaneously - arrays with equal lengths
        var query = """
                    datatable(id: long, arr1: dynamic, arr2: dynamic)
                    [
                        1, dynamic([10, 20]), dynamic(["a", "b"]),
                        2, dynamic([30, 40]), dynamic(["c", "d"])
                    ]
                    | mv-expand arr1, arr2
                    """;

        var expected = """
                       1,10,a
                       1,20,b
                       2,30,c
                       2,40,d
                       """;

        var result = await ResultAsLines(query);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task MvExpand_MultipleColumns_UnequalLengths()
    {
        // Expand multiple columns with arrays of different lengths - shorter arrays padded with null
        var query = """
                    datatable(id: long, arr1: dynamic, arr2: dynamic)
                    [
                        1, dynamic([10, 20, 30]), dynamic(["a", "b"]),
                        2, dynamic([40]), dynamic(["c", "d", "e"])
                    ]
                    | mv-expand arr1, arr2
                    """;

        var expected = """
                       1,10,a
                       1,20,b
                       1,30,<null>
                       2,40,c
                       2,<null>,d
                       2,<null>,e
                       """;

        var result = await ResultAsLines(query);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task MvExpand_Sequence()
    {
        // Test mv-expand called multiple times in sequence: mv-expand ... | mv-expand ...
        var query = """
                    datatable(id: long, arr1: dynamic, arr2: dynamic)
                    [
                        1, dynamic([10, 20]), dynamic(["a", "b"])
                    ]
                    | mv-expand arr1
                    | mv-expand arr2
                    """;

        var expected = """
                       1,10,a
                       1,10,b
                       1,20,a
                       1,20,b
                       """;

        var result = await ResultAsLines(query);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task MvExpand_WithJoin()
    {
        // Test mv-expand as part of a join: mv-expand ... | join (... | mv-expand ...) on ...
        var query = """
                    let LeftTable = datatable(id: long, values: dynamic)
                    [
                        1, dynamic([10, 20]),
                        2, dynamic([30])
                    ];
                    let RightTable = datatable(id: long, names: dynamic)
                    [
                        1, dynamic(["a", "b"]),
                        2, dynamic(["c"])
                    ];
                    LeftTable
                    | mv-expand values
                    | join kind=inner (RightTable | mv-expand names) on id
                    """;

        var expected = """
                       1,10,1,a
                       1,10,1,b
                       1,20,1,a
                       1,20,1,b
                       2,30,2,c
                       """;

        var result = await ResultAsLines(query);
        result.Should().Be(expected);
    }

    [TestMethod]
    public async Task MvExpand_NestedDynamic()
    {
        // Test mv-expand on a nested dynamic property (e.g. properties.ipConfigurations)
        var query = """
                    datatable(id: long, properties: dynamic)
                    [
                        1, dynamic({"ipConfigurations": [{"name": "config1"}, {"name": "config2"}]}),
                        2, dynamic({"ipConfigurations": [{"name": "config3"}]})
                    ]
                    | mv-expand properties.ipConfigurations
                    """;

        var expected = Squash("""
                       1,{"ipConfigurations":[{"name":"config1"},{"name":"config2"}]},{"name":"config1"}
                       1,{"ipConfigurations":[{"name":"config1"},{"name":"config2"}]},{"name":"config2"}
                       2,{"ipConfigurations":[{"name":"config3"}]},{"name":"config3"}
                       """);

        var result = await ResultAsLines(query);
        Squash(result).Should().Be(expected);
    }

    [TestMethod]
    public async Task MvExpand_WithAlias()
    {
        // Test mv-expand with alias on a nested dynamic property
        // Using an alias creates only one new column (the alias), not a properties_ipConfigurations column
        var query = """
                    datatable(id: long, properties: dynamic)
                    [
                        1, dynamic({"ipConfigurations": [{"name": "config1"}, {"name": "config2"}]}),
                        2, dynamic({"ipConfigurations": [{"name": "config3"}]})
                    ]
                    | mv-expand ipConfig = properties.ipConfigurations
                    """;

        var expected = Squash("""
                       1,{"ipConfigurations":[{"name":"config1"},{"name":"config2"}]},{"name":"config1"}
                       1,{"ipConfigurations":[{"name":"config1"},{"name":"config2"}]},{"name":"config2"}
                       2,{"ipConfigurations":[{"name":"config3"}]},{"name":"config3"}
                       """);

        var result = await ResultAsLines(query);
        Squash(result).Should().Be(expected);
    }
}
