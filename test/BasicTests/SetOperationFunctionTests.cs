using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class SetOperationFunctionTests : TestMethods
{
    [TestMethod]
    public async Task SetUnion_TwoArrays() =>
        (await SquashedLastLineOfResult("print set_union(dynamic([1,2,3]), dynamic([3,4,5]))"))
            .Should().Be("[1,2,3,4,5]");

    [TestMethod]
    public async Task SetUnion_ThreeArrays_Dedup() =>
        (await SquashedLastLineOfResult("print set_union(dynamic([1,2]), dynamic([2,3]), dynamic([3,4]))"))
            .Should().Be("[1,2,3,4]");

    [TestMethod]
    public async Task SetIntersect_Common() =>
        (await SquashedLastLineOfResult("print set_intersect(dynamic([1,2,3,4]), dynamic([2,3,5]))"))
            .Should().Be("[2,3]");

    [TestMethod]
    public async Task SetIntersect_ThreeArrays() =>
        (await SquashedLastLineOfResult("print set_intersect(dynamic([1,2,3]), dynamic([2,3,4]), dynamic([3,4,5]))"))
            .Should().Be("[3]");

    [TestMethod]
    public async Task SetDifference_FirstMinusRest() =>
        (await SquashedLastLineOfResult("print set_difference(dynamic([1,2,3,4]), dynamic([2,4]))"))
            .Should().Be("[1,3]");

    [TestMethod]
    public async Task SetDifference_MultipleExcludes() =>
        (await SquashedLastLineOfResult("print set_difference(dynamic([1,2,3,4,5]), dynamic([2]), dynamic([4]))"))
            .Should().Be("[1,3,5]");

    [TestMethod]
    public async Task SetUnion_Strings() =>
        (await SquashedLastLineOfResult("print set_union(dynamic([\"a\",\"b\"]), dynamic([\"b\",\"c\"]))"))
            .Should().Be("[\"a\",\"b\",\"c\"]");
}
