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
