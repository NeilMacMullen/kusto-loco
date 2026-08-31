namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// has_ipv4(source, ip) -> true iff 'source' contains the IPv4 'ip' as a properly delimited term; null when 'ip' is not
// a valid IPv4 address.
[KustoImplementation(Keyword = "Functions.HasIpv4")]
internal partial class HasIpv4Function
{
    private static bool? Impl(string source, string ip) => Ipv4Support.HasIp(source, ip);
}
