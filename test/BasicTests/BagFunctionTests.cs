using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class BagFunctionTests : TestMethods
{
    [TestMethod]
    public async Task BagHasKey_Present() =>
        (await LastLineOfResult("print bag_has_key(dynamic({\"a\":1,\"b\":2}), 'a')")).Should().Be("True");

    [TestMethod]
    public async Task BagHasKey_Absent() =>
        (await LastLineOfResult("print bag_has_key(dynamic({\"a\":1,\"b\":2}), 'z')")).Should().Be("False");

    [TestMethod]
    public async Task BagRemoveKeys() =>
        (await SquashedLastLineOfResult("print bag_remove_keys(dynamic({\"a\":1,\"b\":2,\"c\":3}), dynamic([\"a\",\"c\"]))"))
            .Should().Be("{\"b\":2}");

    [TestMethod]
    public async Task BagSetKey_AddNew() =>
        (await SquashedLastLineOfResult("print bag_set_key(dynamic({\"a\":1}), 'b', 2)"))
            .Should().Be("{\"a\":1,\"b\":2}");

    [TestMethod]
    public async Task BagSetKey_Overwrite() =>
        (await SquashedLastLineOfResult("print bag_set_key(dynamic({\"a\":1}), 'a', 5)"))
            .Should().Be("{\"a\":5}");

    [TestMethod]
    public async Task BagSetKey_StringValue() =>
        (await SquashedLastLineOfResult("print bag_set_key(dynamic({\"a\":1}), 'b', 'x')"))
            .Should().Be("{\"a\":1,\"b\":\"x\"}");

    [TestMethod]
    public async Task BagZip() =>
        (await SquashedLastLineOfResult("print bag_zip(dynamic([\"a\",\"b\",\"c\"]), dynamic([1,2,3]))"))
            .Should().Be("{\"a\":1,\"b\":2,\"c\":3}");
}
