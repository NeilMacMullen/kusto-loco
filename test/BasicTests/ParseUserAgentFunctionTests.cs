using System.Linq;
using AwesomeAssertions;
using KustoLoco.Core;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class ParseUserAgentFunctionTests : TestMethods
{
    private sealed class StubUaParser : IUserAgentParser
    {
        public UserAgentInfo Parse(string userAgent) => new(
            Browser: new UserAgentSoftware("Chrome", "120", "0", "0"),
            OperatingSystem: new UserAgentSoftware("Windows", "10", null, null, null),
            Device: new UserAgentDevice("Other"));
    }

    private static string LastLine(KustoQueryResult result) =>
        result.GetRow(result.RowCount - 1).Select(KustoFormatter.ObjectToKustoString).JoinString();

    [TestMethod]
    public async Task ParseUserAgent_Browser()
    {
        var context = CreateContext();
        context.AddProvider<IUserAgentParser>(new StubUaParser());
        var result = await context.RunQuery("print tostring(parse_user_agent('ua-string', 'browser'))");
        var line = LastLine(result);
        line.Should().Contain("Chrome");
        line.Should().Contain("Browser");
    }

    [TestMethod]
    public async Task ParseUserAgent_BrowserAndOs()
    {
        var context = CreateContext();
        context.AddProvider<IUserAgentParser>(new StubUaParser());
        var result = await context.RunQuery("print tostring(parse_user_agent('ua-string', dynamic([\"browser\",\"os\"])))");
        var line = LastLine(result);
        line.Should().Contain("Chrome");
        line.Should().Contain("Windows");
    }

    [TestMethod]
    public async Task ParseUserAgent_NoProvider_Null()
    {
        var context = CreateContext();
        var result = await context.RunQuery("print parse_user_agent('ua-string', 'browser')");
        LastLine(result).Should().Be("<null>");
    }
}
