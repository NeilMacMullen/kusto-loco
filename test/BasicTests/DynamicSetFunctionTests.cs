using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class DynamicSetFunctionTests : TestMethods
{
    [TestMethod]
    public async Task SetHasElement_Present()
    {
        var query = "print set_has_element(dynamic([1,2,3]), 2)";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task SetHasElement_Absent()
    {
        var query = "print set_has_element(dynamic([1,2,3]), 5)";
        var result = await LastLineOfResult(query);
        result.Should().Be("False");
    }

    [TestMethod]
    public async Task SetHasElement_String()
    {
        var query = "print set_has_element(dynamic([\"a\",\"b\",\"c\"]), \"b\")";
        var result = await LastLineOfResult(query);
        result.Should().Be("True");
    }

    [TestMethod]
    public async Task SetHasElement_StringAbsent()
    {
        var query = "print set_has_element(dynamic([\"a\",\"b\",\"c\"]), \"z\")";
        var result = await LastLineOfResult(query);
        result.Should().Be("False");
    }

    [TestMethod]
    public async Task SetHasElement_NotAnArray_IsNull()
    {
        // set_has_element over a non-array dynamic yields null (renders as empty).
        var query = "print set_has_element(dynamic({\"a\":1}), 1)";
        var result = await LastLineOfResult(query);
        result.Should().Be("");
    }

    [TestMethod]
    public async Task BagKeys_Object()
    {
        var query = "print tostring(bag_keys(dynamic({\"a\":1,\"b\":2,\"c\":3})))";
        var result = await LastLineOfResult(query);
        result.Should().Be("[\"a\",\"b\",\"c\"]");
    }

    [TestMethod]
    public async Task BagKeys_NotAnObject_IsNull()
    {
        var query = "print bag_keys(dynamic([1,2,3]))";
        var result = await LastLineOfResult(query);
        result.Should().Be("");
    }
}
