using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class UrlTests : TestMethods
{
    [TestMethod]
    public async Task UrlEncode()
    {
        var query = """
                       let url = @'https://www.bing.com/hello world';
                    print encoded = url_encode(url)
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be(@"https%3a%2f%2fwww.bing.com%2fhello+world");
    }
}



[TestClass]
public class RegexTests : TestMethods
{
    [TestMethod]
    public async Task RegexQuote()
    {
        var query = """
                      print result = regex_quote('(so$me.Te^xt)')
                    """;
        var result = await LastLineOfResult(query);
        result.Should().Be(@"\(so\$me\.Te\^xt\)");
    }
}
