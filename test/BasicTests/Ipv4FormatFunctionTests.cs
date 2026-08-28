using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class Ipv4FormatFunctionTests : TestMethods
{
    [TestMethod]
    public async Task FormatIpv4_NoPrefix() =>
        (await LastLineOfResult("print format_ipv4('192.168.1.255')")).Should().Be("192.168.1.255");

    [TestMethod]
    public async Task FormatIpv4_Prefix() =>
        (await LastLineOfResult("print format_ipv4('192.168.1.255', 24)")).Should().Be("192.168.1.0");

    [TestMethod]
    public async Task FormatIpv4_BadIp_Null() =>
        (await LastLineOfResult("print format_ipv4('nope', 24)")).Should().Be("<null>");

    [TestMethod]
    public async Task FormatIpv4Mask() =>
        (await LastLineOfResult("print format_ipv4_mask('192.168.1.255', 24)")).Should().Be("192.168.1.0/24");

    [TestMethod]
    public async Task Ipv4NetmaskSuffix_Present() =>
        (await LastLineOfResult("print ipv4_netmask_suffix('192.168.1.1/24')")).Should().Be("24");

    [TestMethod]
    public async Task Ipv4NetmaskSuffix_Absent_Is32() =>
        (await LastLineOfResult("print ipv4_netmask_suffix('192.168.1.1')")).Should().Be("32");

    [TestMethod]
    public async Task ParseIpv4Mask() =>
        (await LastLineOfResult("print parse_ipv4_mask('192.168.1.255', 24)")).Should().Be("3232235776");
}
