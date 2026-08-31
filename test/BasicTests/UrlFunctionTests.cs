using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class UrlFunctionTests : TestMethods
{
    [TestMethod]
    public async Task ParseUrl_FullUrl() =>
        (await SquashedLastLineOfResult(
            "print parse_url('scheme://username:password@host:1234/this/is/a/path?k1=v1&k2=v2#fragment')"))
            .Should().Be(
                "{\"Scheme\":\"scheme\",\"Host\":\"host\",\"Port\":\"1234\",\"Path\":\"/this/is/a/path\"," +
                "\"Username\":\"username\",\"Password\":\"password\"," +
                "\"QueryParameters\":{\"k1\":\"v1\",\"k2\":\"v2\"},\"Fragment\":\"fragment\"}");

    [TestMethod]
    public async Task ParseUrl_MinimalHostPath() =>
        (await SquashedLastLineOfResult("print parse_url('https://contoso.com/a/b')"))
            .Should().Be(
                "{\"Scheme\":\"https\",\"Host\":\"contoso.com\",\"Port\":\"\",\"Path\":\"/a/b\"," +
                "\"Username\":\"\",\"Password\":\"\",\"QueryParameters\":{},\"Fragment\":\"\"}");

    [TestMethod]
    public async Task ParseUrlQuery() =>
        (await SquashedLastLineOfResult("print parse_urlquery('k1=v1&k2=v2&k3=v3')"))
            .Should().Be("{\"QueryParameters\":{\"k1\":\"v1\",\"k2\":\"v2\",\"k3\":\"v3\"}}");
}
