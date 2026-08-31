using System.Linq;
using System.Net;
using AwesomeAssertions;
using KustoLoco.Core;
using KustoLoco.Geo;

namespace GeoTests;

[TestClass]
public class DbIpGeoProviderTests
{
    // DB-IP City Lite layout: ip_start,ip_end,continent,country,stateprov,city,latitude,longitude
    private static readonly string[] Csv =
    {
        "1.0.0.0,1.0.0.255,OC,AU,Queensland,Brisbane,-27.4679,153.0281",
        "8.8.8.0,8.8.8.255,NA,US,California,Mountain View,37.386,-122.0838",
        "2001:4860:4860::,2001:4860:4860:ffff:ffff:ffff:ffff:ffff,NA,US,California,Mountain View,37.386,-122.0838",
    };

    [TestMethod]
    public void ResolvesIpv4_MapsIsoCodeToCountryName()
    {
        var provider = DbIpGeoProvider.FromLines(Csv);
        var info = provider.Lookup(IPAddress.Parse("8.8.8.8"));
        info.Should().NotBeNull();
        info!.Country.Should().Be("United States"); // ISO 'US' -> English name via RegionInfo
        info.State.Should().Be("California");
        info.City.Should().Be("Mountain View");
        info.Latitude.Should().BeApproximately(37.386, 0.001);
        info.Longitude.Should().BeApproximately(-122.0838, 0.001);
    }

    [TestMethod]
    public void ResolvesIpv6()
    {
        var provider = DbIpGeoProvider.FromLines(Csv);
        var info = provider.Lookup(IPAddress.Parse("2001:4860:4860::8888"));
        info.Should().NotBeNull();
        info!.City.Should().Be("Mountain View");
    }

    [TestMethod]
    public void UncoveredAddress_YieldsNull()
    {
        var provider = DbIpGeoProvider.FromLines(Csv);
        provider.Lookup(IPAddress.Parse("203.0.113.1")).Should().BeNull();
    }

    [TestMethod]
    public void SkipsHeaderAndBlankLines()
    {
        var withNoise = new[] { "ip_start,ip_end,continent,country,stateprov,city,latitude,longitude", "", "# comment" }
            .Concat(Csv);
        var provider = DbIpGeoProvider.FromLines(withNoise);
        provider.RangeCount.Should().Be(3);
    }

    [TestMethod]
    public async Task EndToEnd_GeoFunctionResolvesThroughRegisteredProvider()
    {
        var context = new KustoQueryContext();
        context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.FromLines(Csv));
        var result = await context.RunQuery("print g = tostring(geo_info_from_ip_address('8.8.8.8'))");
        var rendered = result.GetRow(0).First()?.ToString() ?? string.Empty;
        rendered.Should().Contain("United States").And.Contain("Mountain View");
    }

    [TestMethod]
    public async Task EndToEnd_NoProvider_YieldsNull()
    {
        var context = new KustoQueryContext();
        var result = await context.RunQuery("print g = isnull(geo_info_from_ip_address('8.8.8.8'))");
        result.GetRow(0).First().Should().Be(true);
    }
}
