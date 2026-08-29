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

    // The compact "country + centroid" layout (ip_start,ip_end,country,latitude,longitude): country-level geo WITH
    // coordinates, auto-detected by its 5 columns. Country still maps to the English name; State/City are null.
    private static readonly string[] CountryCentroidCsv =
    {
        "1.0.0.0,1.0.0.255,AU,-25.0,133.0",
        "8.8.8.0,8.8.8.255,US,39.0,-98.0",
    };

    [TestMethod]
    public void CountryCentroidLayout_AutoDetected_ResolvesCountryAndCoordinates()
    {
        var provider = DbIpGeoProvider.FromLines(CountryCentroidCsv); // Auto
        var info = provider.Lookup(IPAddress.Parse("8.8.8.8"));
        info.Should().NotBeNull();
        info!.Country.Should().Be("United States"); // ISO 'US' -> English name, same as City Lite
        info.State.Should().BeNull();
        info.City.Should().BeNull();
        info.Latitude.Should().BeApproximately(39.0, 0.001);
        info.Longitude.Should().BeApproximately(-98.0, 0.001);
    }

    [TestMethod]
    public void CountryCentroidLayout_SkipsHeaderRow()
    {
        var withHeader = new[] { "startIp,endIp,countryCode,latitude,longitude" }.Concat(CountryCentroidCsv);
        var provider = DbIpGeoProvider.FromLines(withHeader); // Auto — header's first field is not an IP
        provider.RangeCount.Should().Be(2);
        provider.Lookup(IPAddress.Parse("1.0.0.1"))!.Country.Should().Be("Australia");
    }

    // DB-IP Country Lite (ip_start,ip_end,country): country only, no coordinates. Auto-detected by its 3 columns.
    private static readonly string[] CountryLiteCsv =
    {
        "1.0.0.0,1.0.0.255,AU",
        "8.8.8.0,8.8.8.255,US",
    };

    [TestMethod]
    public void CountryLiteLayout_AutoDetected_ResolvesCountryWithNullCoordinates()
    {
        var provider = DbIpGeoProvider.FromLines(CountryLiteCsv); // Auto
        var info = provider.Lookup(IPAddress.Parse("8.8.8.8"));
        info.Should().NotBeNull();
        info!.Country.Should().Be("United States");
        info.Latitude.Should().BeNull();
        info.Longitude.Should().BeNull();
    }

    // The whole point of embedding a default dataset: a host registers ONE line and geo_info_from_ip_address
    // resolves — no file, no path, no config (the same contract UapUserAgentParser.Default gives parse_user_agent).
    [TestMethod]
    public void Default_ResolvesFromTheEmbeddedDataset_WithNoHostFile()
    {
        var provider = DbIpGeoProvider.Default;
        provider.RangeCount.Should().BeGreaterThan(100_000); // the real DB-IP country set, not a stub
        var info = provider.Lookup(IPAddress.Parse("8.8.8.8"));
        info.Should().NotBeNull();
        info!.Country.Should().Be("United States"); // ADX shape: English name, not the ISO code
        info.Latitude.Should().NotBeNull();
        info.Longitude.Should().NotBeNull();
    }

    [TestMethod]
    public async Task EndToEnd_DefaultProvider_ResolvesGeoWithZeroConfiguration()
    {
        var context = new KustoQueryContext();
        context.AddProvider<IGeoIpProvider>(DbIpGeoProvider.Default);
        var result = await context.RunQuery(
            "print c = tostring(geo_info_from_ip_address('8.8.8.8').country)");
        result.GetRow(0).First()?.ToString().Should().Be("United States");
    }

    [TestMethod]
    public void ExplicitLayout_OverridesDetection()
    {
        // Forcing CountryCentroid on a 5-column file is a no-op vs Auto, but proves the explicit path maps the same.
        var provider = DbIpGeoProvider.FromLines(CountryCentroidCsv, DbIpLayout.CountryCentroid);
        provider.Lookup(IPAddress.Parse("1.0.0.1"))!.Latitude.Should().BeApproximately(-25.0, 0.001);
    }
}
