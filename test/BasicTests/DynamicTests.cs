using FluentAssertions;

namespace BasicTests;

[TestClass]
public class DynamicTests :TestMethods
{

    [TestMethod]
    public async Task MemberAccess()
    {
        var query = @"print o=dynamic({""a"":123}) | project z=o.a";
        var result = await LastLineOfResult(query);
        result.Should().Be("123");
    }

    [TestMethod]
    public async Task NestedMemberAccess()
    {
        var query = @"print o=dynamic({""a"":{""b"":456}}) | project z=o.a.b";
        var result = await LastLineOfResult(query);
        result.Should().Be("456");
    }

    [TestMethod]
    public async Task ArrayAccess()
    {
        var query = @"print o=dynamic({""a"":[1,2]}) | project z=o.a[0]";
        var result = await LastLineOfResult(query);
        result.Should().Be("1");
    }

}