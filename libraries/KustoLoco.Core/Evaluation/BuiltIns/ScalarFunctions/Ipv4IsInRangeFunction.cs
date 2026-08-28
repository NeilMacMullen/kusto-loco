namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ipv4_is_in_range(ip, ip_range) -> true iff the IPv4 'ip' is inside the CIDR 'ip_range'. Null on unparseable/malformed.
[KustoImplementation(Keyword = "Functions.Ipv4IsInRange")]
internal partial class Ipv4IsInRangeFunction
{
    private static bool? Impl(string ip, string cidr) => Ipv4Support.InRange(ip, cidr);
}
