namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// has_ipv4_prefix(source, ip_prefix) -> true iff 'source' contains an IPv4 term whose leading octets match 'ip_prefix'
// (e.g. "10.1."); null when the prefix is malformed.
[KustoImplementation(Keyword = "Functions.HasIpv4Prefix")]
internal partial class HasIpv4PrefixFunction
{
    private static bool? Impl(string source, string ipPrefix) => Ipv4Support.HasIpPrefix(source, ipPrefix);
}
