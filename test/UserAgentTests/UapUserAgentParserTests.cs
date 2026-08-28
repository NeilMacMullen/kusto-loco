using System.Linq;
using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.UserAgent;

namespace UserAgentTests;

[TestClass]
public class UapUserAgentParserTests
{
    private const string ChromeOnWindows =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
    private const string SafariOnIphone =
        "Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Mobile/15E148 Safari/604.1";

    private static readonly UapUserAgentParser Parser = UapUserAgentParser.Default;

    [TestMethod]
    public void EntireCanonicalDatasetCompiles()
    {
        // Every uap-core regex compiles under .NET's engine, so no coverage is silently lost.
        Parser.SkippedRegexCount.Should().Be(0);
    }

    [TestMethod]
    public void ParsesBrowserFamilyAndVersion()
    {
        var info = Parser.Parse(ChromeOnWindows);
        info.Browser.Family.Should().Be("Chrome");
        info.Browser.Major.Should().Be("120");
        info.OperatingSystem.Family.Should().Be("Windows");
        info.OperatingSystem.Major.Should().Be("10");
        info.Device.Family.Should().Be("Other"); // a desktop has no device
    }

    [TestMethod]
    public void ParsesDeviceBrandAndModel()
    {
        var info = Parser.Parse(SafariOnIphone);
        info.Browser.Family.Should().Be("Mobile Safari");
        info.OperatingSystem.Family.Should().Be("iOS");
        info.OperatingSystem.Major.Should().Be("14");
        info.OperatingSystem.Minor.Should().Be("6");
        info.Device.Family.Should().Be("iPhone");
        info.Device.Brand.Should().Be("Apple");
        info.Device.Model.Should().Be("iPhone");
    }

    [TestMethod]
    public void ParsesNonBrowserClient()
    {
        var info = Parser.Parse("curl/8.4.0");
        info.Browser.Family.Should().Be("curl");
        info.Browser.Major.Should().Be("8");
    }

    [TestMethod]
    public void UnknownUserAgentYieldsOther()
    {
        var info = Parser.Parse("this is not a user agent");
        info.Browser.Family.Should().Be("Other");
        info.OperatingSystem.Family.Should().Be("Other");
        info.Device.Family.Should().Be("Other");
    }

    [TestMethod]
    public async Task EndToEnd_BrowserResolvesThroughRegisteredProvider()
    {
        var context = new KustoQueryContext();
        context.AddProvider<IUserAgentParser>(UapUserAgentParser.Default);
        var result = await context.RunQuery(
            $"print b = tostring(parse_user_agent('{ChromeOnWindows}', 'browser'))");
        var rendered = result.GetRow(0).First()?.ToString() ?? string.Empty;
        rendered.Should().Contain("Chrome").And.Contain("120");
    }

    [TestMethod]
    public async Task EndToEnd_DeviceResolvesBrandAndModel()
    {
        var context = new KustoQueryContext();
        context.AddProvider<IUserAgentParser>(UapUserAgentParser.Default);
        var result = await context.RunQuery(
            $"print d = tostring(parse_user_agent('{SafariOnIphone}', 'device'))");
        var rendered = result.GetRow(0).First()?.ToString() ?? string.Empty;
        rendered.Should().Contain("iPhone").And.Contain("Apple");
    }

    [TestMethod]
    public async Task EndToEnd_NoProvider_YieldsNull()
    {
        var context = new KustoQueryContext();
        var result = await context.RunQuery(
            $"print n = isnull(parse_user_agent('{ChromeOnWindows}', 'browser'))");
        result.GetRow(0).First().Should().Be(true);
    }
}
