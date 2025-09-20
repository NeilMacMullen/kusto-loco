using AwesomeAssertions;

namespace BasicTests;

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
