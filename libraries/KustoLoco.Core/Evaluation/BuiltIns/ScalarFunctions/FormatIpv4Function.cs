namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// format_ipv4(ip [, prefix]) -> the network address masked to the effective prefix (default 32) as dotted-quad; null
// when the address is unparseable or the prefix is out of range.
[KustoImplementation(Keyword = "Functions.FormatIPV4")]
internal partial class FormatIpv4Function
{
    private static string? Impl(string ip) => Ipv4Support.FormatV4(ip, null);
    private static string? PrefixImpl(string ip, long prefix) => Ipv4Support.FormatV4(ip, prefix);
}
