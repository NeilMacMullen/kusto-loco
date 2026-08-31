using System.Linq;
using System.Net;
using AwesomeAssertions;
using KustoLoco.Core;
using NotNullStrings;

namespace BasicTests;

[TestClass]
public class GeoInfoFromIpFunctionTests : TestMethods
{
    private sealed class StubGeoProvider : IGeoIpProvider
    {
        public GeoIpInfo? Lookup(IPAddress address) =>
            address.ToString() == "8.8.8.8"
                ? new GeoIpInfo("United States", "California", "Mountain View", 37.386, -122.0838)
                : null;
    }

    private static string LastLine(KustoQueryResult result) =>
        result.GetRow(result.RowCount - 1).Select(KustoFormatter.ObjectToKustoString).JoinString();

    [TestMethod]
    public async Task GeoInfoFromIp_WithProvider_Resolves()
    {
        var context = CreateContext();
        context.AddProvider<IGeoIpProvider>(new StubGeoProvider());
        var result = await context.RunQuery("print tostring(geo_info_from_ip_address('8.8.8.8'))");
        var line = LastLine(result);
        line.Should().Contain("United States");
        line.Should().Contain("Mountain View");
    }

    [TestMethod]
    public async Task GeoInfoFromIp_UnknownAddress_Null()
    {
        var context = CreateContext();
        context.AddProvider<IGeoIpProvider>(new StubGeoProvider());
        var result = await context.RunQuery("print geo_info_from_ip_address('1.2.3.4')");
        LastLine(result).Should().Be("<null>");
    }

    [TestMethod]
    public async Task GeoInfoFromIp_NoProvider_InertNull()
    {
        var context = CreateContext();   // no provider registered -> function stays inert
        var result = await context.RunQuery("print geo_info_from_ip_address('8.8.8.8')");
        LastLine(result).Should().Be("<null>");
    }
}
