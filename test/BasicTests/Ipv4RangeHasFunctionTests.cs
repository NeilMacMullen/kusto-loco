using AwesomeAssertions;

namespace BasicTests;

[TestClass]
public class Ipv4RangeHasFunctionTests : TestMethods
{
    [TestMethod]
    public async Task RangeToCidr_AlignedBlock() =>
        (await SquashedLastLineOfResult("print ipv4_range_to_cidr_list('192.168.1.0', '192.168.1.255')"))
            .Should().Be("[\"192.168.1.0/24\"]");

    [TestMethod]
    public async Task RangeToCidr_SmallBlock() =>
        (await SquashedLastLineOfResult("print ipv4_range_to_cidr_list('10.0.0.0', '10.0.0.3')"))
            .Should().Be("[\"10.0.0.0/30\"]");

    [TestMethod]
    public async Task RangeToCidr_UnalignedSplit() =>
        (await SquashedLastLineOfResult("print ipv4_range_to_cidr_list('10.0.0.1', '10.0.0.2')"))
            .Should().Be("[\"10.0.0.1/32\",\"10.0.0.2/32\"]");

    [TestMethod]
    public async Task HasIpv4_Delimited() =>
        (await LastLineOfResult("print has_ipv4('Source address 10.1.2.3 flagged', '10.1.2.3')")).Should().Be("True");

    [TestMethod]
    public async Task HasIpv4_NotDelimited_False() =>
        (await LastLineOfResult("print has_ipv4('address 110.1.2.3 here', '10.1.2.3')")).Should().Be("False");

    [TestMethod]
    public async Task HasIpv4Prefix() =>
        (await LastLineOfResult("print has_ipv4_prefix('conn to 10.1.2.3 open', '10.1.')")).Should().Be("True");

    [TestMethod]
    public async Task HasAnyIpv4_Match() =>
        (await LastLineOfResult("print has_any_ipv4('hit 10.0.0.5 now', dynamic([\"192.168.0.1\",\"10.0.0.5\"]))")).Should().Be("True");

    [TestMethod]
    public async Task HasAnyIpv4Prefix_Match() =>
        (await LastLineOfResult("print has_any_ipv4_prefix('hit 10.0.0.5 now', dynamic([\"192.168.\",\"10.0.\"]))")).Should().Be("True");
}
