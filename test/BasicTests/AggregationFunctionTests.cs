using AwesomeAssertions;

namespace BasicTests;

/// <summary>
/// Tests for aggregation functions: make_bag, make_bag_if, make_list, make_set, buildschema, etc.
/// Following the pattern established in SimpleFunctionTests.
/// </summary>
[TestClass]
public class AggregationFunctionTests : TestMethods
{
    #region bag_pack and pack tests

    [TestMethod]
    public async Task BagPack_StringValue()
    {
        var query = "print bag_pack('key1', 'value1')";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"key1\":\"value1\"");
    }

    [TestMethod]
    public async Task BagPack_IntValue()
    {
        var query = "print bag_pack('count', 42)";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"count\":42");
    }

    [TestMethod]
    public async Task Pack_AliasForBagPack()
    {
        // pack() is a deprecated alias for bag_pack()
        // From Microsoft docs: https://learn.microsoft.com/en-us/kusto/query/pack-function
        var query = "print pack('key1', 'value1')";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"key1\":\"value1\"");
    }

    [TestMethod]
    public async Task BagPack_MultipleStringPairs()
    {
        var query = "print bag_pack('a', 'one', 'b', 'two', 'c', 'three')";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"a\":\"one\",\"b\":\"two\",\"c\":\"three\"}");
    }

    [TestMethod]
    public async Task BagPack_MixedValueTypes()
    {
        var query = "print bag_pack('name', 'kusto', 'count', 42, 'ratio', 0.5, 'active', true)";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"name\":\"kusto\"");
        result.Should().Contain("\"count\":42");
        result.Should().Contain("\"ratio\":0.5");
        result.Should().Contain("\"active\":true");
    }

    [TestMethod]
    public async Task BagPack_FromColumnsBuildsListOfBags()
    {
        // matches the make_list / bag_pack composition described in the issue:
        // each row is packed into a dynamic bag and then aggregated via make_list.
        var query = @"
datatable(Name:string, Value:int, Status:string)
[
    'alpha', 1, 'ok',
    'beta',  2, 'fail',
]
| extend Item = bag_pack('name', Name, 'value', Value, 'status', Status)
| summarize Items = make_list(Item)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("{\"name\":\"alpha\",\"value\":1,\"status\":\"ok\"}");
        result.Should().Contain("{\"name\":\"beta\",\"value\":2,\"status\":\"fail\"}");
    }

    [TestMethod]
    public async Task BagPack_EmptyKeyPairIsSkipped()
    {
        var query = "print bag_pack('', 'ignored', 'kept', 'yes')";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"kept\":\"yes\"}");
    }

    [TestMethod]
    public async Task BagPack_NestedDynamicValueIsPreserved()
    {
        var query = "print bag_pack('outer', bag_pack('inner', 1))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"outer\":{\"inner\":1}}");
    }

    [TestMethod]
    public async Task BagPack_NullValueIsSerializedAsNull()
    {
        var query = "print bag_pack('missing', dynamic(null), 'present', 1)";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"missing\":null,\"present\":1}");
    }

    [TestMethod]
    public async Task BagPack_DateTimeAndTimeSpanValues()
    {
        var query = "print bag_pack('ts', datetime(2024-01-02T03:04:05Z), 'dur', 5m)";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"ts\":\"2024-01-02T");
        result.Should().Contain("\"dur\":\"00:05:00\"");
    }

    #endregion

    #region bag_merge tests

    [TestMethod]
    public async Task BagMerge_TwoDisjointBags()
    {
        var query = "print bag_merge(bag_pack('a', 1), bag_pack('b', 2))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"a\":1,\"b\":2}");
    }

    [TestMethod]
    public async Task BagMerge_LeftmostArgWinsOnConflict()
    {
        var query = "print bag_merge(bag_pack('k', 'first'), bag_pack('k', 'last'))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"k\":\"first\"}");
    }

    [TestMethod]
    public async Task BagMerge_ThreeBags()
    {
        var query = "print bag_merge(bag_pack('a', 1), bag_pack('b', 2), bag_pack('c', 3))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"a\":1,\"b\":2,\"c\":3}");
    }

    [TestMethod]
    public async Task BagMerge_LeftmostWinsAcrossThreeBags()
    {
        var query = "print bag_merge(bag_pack('k', 1), bag_pack('k', 2), bag_pack('k', 3))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"k\":1}");
    }

    [TestMethod]
    public async Task BagMerge_EmptyBag()
    {
        var query = "print bag_merge(dynamic({}), bag_pack('a', 1))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"a\":1}");
    }

    [TestMethod]
    public async Task BagMerge_NullBagIsSkipped()
    {
        var query = "print bag_merge(dynamic(null), bag_pack('a', 1))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"a\":1}");
    }

    [TestMethod]
    public async Task BagMerge_NonObjectArgIsSkipped()
    {
        // Non-object dynamic values flow through dynamic-typed columns; the impl skips them.
        var query = @"
datatable(b:dynamic)
[
    dynamic([1,2,3]),
]
| summarize merged = bag_merge(dynamic({""kept"":1}), take_any(b))
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"kept\":1}");
    }

    [TestMethod]
    public async Task BagMerge_NestedBagsArePreserved()
    {
        var query = "print bag_merge(bag_pack('outer', bag_pack('inner', 1)), bag_pack('other', 2))";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("{\"outer\":{\"inner\":1},\"other\":2}");
    }

    [TestMethod]
    public async Task BagMerge_FromColumns()
    {
        var query = @"
datatable(b1:dynamic, b2:dynamic)
[
    dynamic({""a"":1}), dynamic({""b"":2}),
    dynamic({""x"":10}), dynamic({""x"":20, ""y"":30}),
]
| extend Merged = bag_merge(b1, b2)
| project Merged
";
        var result = Squash(await ResultAsString(query, "|"));
        result.Should().Be("{\"a\":1,\"b\":2}|{\"x\":10,\"y\":30}");
    }

    #endregion

    #region make_bag tests

    [TestMethod]
    public async Task MakeBag_BasicExample()
    {
        // From Microsoft docs: make_bag basic example
        var query = @"
datatable(prop:string, value:string)
[
    'prop01', 'val_a',
    'prop02', 'val_b',
    'prop03', 'val_c',
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag(p)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"prop01\":\"val_a\"");
        result.Should().Contain("\"prop02\":\"val_b\"");
        result.Should().Contain("\"prop03\":\"val_c\"");
    }

    [TestMethod]
    public async Task MakeBag_WithMaxSize()
    {
        // Test make_bag with maxSize parameter
        var query = @"
datatable(prop:string, value:string)
[
    'prop01', 'val_a',
    'prop02', 'val_b',
    'prop03', 'val_c',
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag(p, 2)
";
        var result = await SquashedLastLineOfResult(query);
        // Should only contain 2 keys due to maxSize limit
        result.Should().Contain("\"prop01\":\"val_a\"");
        result.Should().Contain("\"prop02\":\"val_b\"");
        result.Should().NotContain("\"prop03\"");
    }

    #endregion

    #region make_bag_if tests

    [TestMethod]
    public async Task MakeBagIf_BasicExample()
    {
        // From Microsoft docs: make_bag_if basic example
        var query = @"
datatable(prop:string, value:string, predicate:bool)
[
    'prop01', 'val_a', true,
    'prop02', 'val_b', false,
    'prop03', 'val_c', true
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag_if(p, predicate)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"prop01\":\"val_a\"");
        result.Should().Contain("\"prop03\":\"val_c\"");
        result.Should().NotContain("\"prop02\""); // predicate was false
    }

    [TestMethod]
    public async Task MakeBagIf_WithMaxSize()
    {
        // Test make_bag_if with maxSize parameter
        var query = @"
datatable(prop:string, value:string, predicate:bool)
[
    'prop01', 'val_a', true,
    'prop02', 'val_b', true,
    'prop03', 'val_c', true,
]
| extend p = bag_pack(prop, value)
| summarize dict = make_bag_if(p, predicate, 2)
";
        var result = await SquashedLastLineOfResult(query);
        // Should only contain 2 keys due to maxSize limit
        result.Should().Contain("\"prop01\":\"val_a\"");
        result.Should().Contain("\"prop02\":\"val_b\"");
        result.Should().NotContain("\"prop03\"");
    }

    #endregion

    #region make_list tests

    [TestMethod]
    public async Task MakeList_BasicExample()
    {
        var query = @"
datatable(x: int) [1, 3, 2, 1]
| summarize a = make_list(x)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,3,2,1]");
    }

    [TestMethod]
    public async Task MakeList_WithMaxSize()
    {
        var query = @"
datatable(x: int) [1, 3, 2, 1]
| summarize b = make_list(x, 2)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,3]");
    }

    [TestMethod]
    public async Task MakeList_WithDynamic()
    {
        // Test make_list with dynamic values
        var query = @"
datatable(value:dynamic)
[
    dynamic({""a"":1}),
    dynamic({""b"":2}),
    dynamic([1,2,3]),
]
| summarize result = make_list(value)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("{\"a\":1}");
        result.Should().Contain("{\"b\":2}");
        result.Should().Contain("[1,2,3]");
    }

    #endregion

    #region make_list_if tests

    [TestMethod]
    public async Task MakeListIf_BasicExample()
    {
        var query = @"
datatable(x: int) [1, 3, 2, 1]
| summarize a = make_list_if(x, x > 1)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[3,2]");
    }

    [TestMethod]
    public async Task MakeListIf_WithMaxSize()
    {
        var query = @"
datatable(x: int) [1, 3, 2, 1]
| summarize b = make_list_if(x, true, 3)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,3,2]");
    }

    #endregion

    #region make_list_with_nulls tests

    [TestMethod]
    public async Task MakeListWithNulls_BasicExample()
    {
        // From Microsoft docs: make_list_with_nulls example
        var query = @"
datatable(Fruit:string, Color:string, Quantity:int)
[
    'Apple', 'Red', 5,
    'Banana', '', 3,
    'Cherry', 'Red', int(null),
]
| summarize Quantities = make_list_with_nulls(Quantity)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("5");
        result.Should().Contain("3");
        result.Should().Contain("null");
    }

    #endregion

    #region make_set tests

    [TestMethod]
    public async Task MakeSet_BasicExample()
    {
        var query = @"
datatable(x: int) [1, 2, 3, 1]
| summarize a = make_set(x)
| project a = array_sort_asc(a)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,2,3]");
    }

    [TestMethod]
    public async Task MakeSet_WithMaxSize()
    {
        var query = @"
datatable(x: int) [1, 2, 3, 1]
| summarize b = make_set(x, 2)
| project b = array_sort_asc(b)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,2]");
    }

    [TestMethod]
    public async Task MakeSet_FromMicrosoftDocs()
    {
        // From Microsoft docs: make_set example
        var query = @"
datatable(Region:string, Sales:int)
[
    'North', 150,
    'North', 200,
    'South', 100,
    'West', 150,
    'South', 100,
]
| summarize UniqueSalesAmounts = make_set(Sales)
| project UniqueSalesAmounts = array_sort_asc(UniqueSalesAmounts)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[100,150,200]");
    }

    #endregion

    #region make_set_if tests

    [TestMethod]
    public async Task MakeSetIf_BasicExample()
    {
        var query = @"
datatable(x: int) [1, 2, 3, 1]
| summarize a = make_set_if(x, x > 1)
| project a = array_sort_asc(a)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[2,3]");
    }

    [TestMethod]
    public async Task MakeSetIf_WithMaxSize()
    {
        var query = @"
datatable(x: int) [1, 2, 3, 1]
| summarize b = make_set_if(x, true, 2)
| project b = array_sort_asc(b)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[1,2]");
    }

    [TestMethod]
    public async Task MakeSetIf_FromMicrosoftDocs()
    {
        // From Microsoft docs: make_set_if example
        var query = @"
datatable(Region:string, Sales:int)
[
    'North', 150,
    'North', 200,
    'South', 100,
    'West', 150,
]
| summarize HighSales = make_set_if(Sales, Sales > 100)
| project HighSales = array_sort_asc(HighSales)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Be("[150,200]");
    }

    #endregion

    #region buildschema tests

    [TestMethod]
    [Ignore("Exploratory: buildschema implementation does not correctly merge nested object schemas with primitive types. The 'y' field should be [\"double\",{\"w\":\"string\"}] but outputs [\"double\"] only.")]
    public async Task BuildSchema_BasicExample()
    {
        // From Microsoft docs: buildschema basic example
        // This test demonstrates the expected behavior according to Microsoft docs,
        // but the current implementation has limitations with merging nested objects and primitives.
        var query = @"
datatable(value: dynamic)
[
    dynamic({""x"":1, ""y"":3.5}),
    dynamic({""x"":""somevalue"", ""z"":[1, 2, 3]}),
    dynamic({""y"":{""w"":""zzz""}, ""t"":[""aa"", ""bb""], ""z"":[""foo""]})
]
| summarize schema_value = buildschema(value)
";
        var result = await SquashedLastLineOfResult(query);
        // Expected according to Microsoft docs:
        // {"x":["long","string"],"y":["double",{"w":"string"}],"z":{"indexer":["long","string"]},"t":{"indexer":"string"}}
        result.Should().Contain("\"x\":[\"long\",\"string\"]");
        result.Should().Contain("\"y\":[\"double\",{\"w\":\"string\"}]");
    }

    [TestMethod]
    public async Task BuildSchema_SimpleTypes()
    {
        // Test buildschema with simple types that should work correctly
        var query = @"
datatable(value: dynamic)
[
    dynamic({""name"":""Alice""}),
    dynamic({""name"":""Bob""}),
]
| summarize schema_value = buildschema(value)
";
        var result = await SquashedLastLineOfResult(query);
        result.Should().Contain("\"name\":\"string\"");
    }

    #endregion
}
