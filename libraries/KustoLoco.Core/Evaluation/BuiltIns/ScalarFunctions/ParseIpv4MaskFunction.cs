namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// parse_ipv4_mask(ip, prefix) -> the network address masked to the effective prefix as a long; null when the address is
// unparseable or the prefix is out of range.
[KustoImplementation(Keyword = "Functions.ParseIPV4Mask")]
internal partial class ParseIpv4MaskFunction
{
    private static long? Impl(string ip, long prefix) => Ipv4Support.ParseV4Mask(ip, prefix);
}
