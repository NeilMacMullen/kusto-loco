namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ipv4_is_match(ip1, ip2 [, prefix]) -> true iff two IPv4 addresses match over the effective prefix (default 32; each
// operand may also carry its own /prefix). Null when either address is unparseable.
[KustoImplementation(Keyword = "Functions.Ipv4IsMatch")]
internal partial class Ipv4IsMatchFunction
{
    private static bool? Impl(string ip1, string ip2) => Ipv4Support.MaskedEqual(ip1, ip2, null);
    private static bool? PrefixImpl(string ip1, string ip2, long prefix) => Ipv4Support.MaskedEqual(ip1, ip2, prefix);
}
