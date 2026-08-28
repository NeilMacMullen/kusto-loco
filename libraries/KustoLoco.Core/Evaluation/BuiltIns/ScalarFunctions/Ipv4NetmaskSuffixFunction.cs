namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ipv4_netmask_suffix(ip_range) -> the /suffix of a CIDR (32 when none present); null when the address is unparseable.
[KustoImplementation(Keyword = "Functions.Ipv4NetmaskSuffix")]
internal partial class Ipv4NetmaskSuffixFunction
{
    private static long? Impl(string ipRange) => Ipv4Support.NetmaskSuffix(ipRange);
}
