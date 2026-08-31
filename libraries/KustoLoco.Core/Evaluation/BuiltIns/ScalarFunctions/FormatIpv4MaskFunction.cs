namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// format_ipv4_mask(ip, prefix) -> the masked network address in CIDR notation; null when the address is unparseable or
// the prefix is out of range.
[KustoImplementation(Keyword = "Functions.FormatIPV4Mask")]
internal partial class FormatIpv4MaskFunction
{
    private static string? Impl(string ip, long prefix) => Ipv4Support.FormatV4Mask(ip, prefix);
}
