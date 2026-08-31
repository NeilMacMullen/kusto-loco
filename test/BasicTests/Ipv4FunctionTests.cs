using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class Ipv4FunctionTests : TestMethods
{
    [TestMethod]
    public async Task Ipv4IsInRange_In() =>
        (await LastLineOfResult("print ipv4_is_in_range('192.168.1.6', '192.168.1.0/24')")).Should().Be("True");

    [TestMethod]
    public async Task Ipv4IsInRange_Out() =>
        (await LastLineOfResult("print ipv4_is_in_range('192.168.2.6', '192.168.1.0/24')")).Should().Be("False");

    [TestMethod]
    public async Task Ipv4IsInRange_BadIp_Null() =>
        (await LastLineOfResult("print ipv4_is_in_range('not-an-ip', '192.168.1.0/24')")).Should().Be("<null>");

    [TestMethod]
    public async Task Ipv4Compare_Equal() =>
        (await LastLineOfResult("print ipv4_compare('192.168.1.1', '192.168.1.1')")).Should().Be("0");

    [TestMethod]
    public async Task Ipv4Compare_Less() =>
        (await LastLineOfResult("print ipv4_compare('192.168.1.1', '192.168.1.2')")).Should().Be("-1");

    [TestMethod]
    public async Task Ipv4Compare_PrefixMasksDifference() =>
        (await LastLineOfResult("print ipv4_compare('192.168.1.1', '192.168.1.2', 24)")).Should().Be("0");

    [TestMethod]
    public async Task Ipv4IsMatch_SamePrefix() =>
        (await LastLineOfResult("print ipv4_is_match('192.168.1.1', '192.168.1.2', 24)")).Should().Be("True");

    [TestMethod]
    public async Task Ipv4IsMatch_DifferentPrefix() =>
        (await LastLineOfResult("print ipv4_is_match('192.168.1.1', '192.168.2.1', 24)")).Should().Be("False");

    [TestMethod]
    public async Task Ipv4IsInAnyRange_MatchCidr() =>
        (await LastLineOfResult("print ipv4_is_in_any_range('10.0.0.5', dynamic([\"192.168.0.0/16\",\"10.0.0.0/8\"]))")).Should().Be("True");

    [TestMethod]
    public async Task Ipv4IsInAnyRange_MatchBareAddress() =>
        (await LastLineOfResult("print ipv4_is_in_any_range('192.168.1.6', dynamic([\"192.168.1.6\",\"10.0.0.0/8\"]))")).Should().Be("True");

    [TestMethod]
    public async Task Ipv4IsInAnyRange_NoMatch() =>
        (await LastLineOfResult("print ipv4_is_in_any_range('172.16.0.1', dynamic([\"192.168.0.0/16\",\"10.0.0.0/8\"]))")).Should().Be("False");
}
