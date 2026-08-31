namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ipv4_compare(ip1, ip2 [, prefix]) -> -1/0/1 comparing two IPv4 addresses over the effective prefix (default 32; each
// operand may also carry its own /prefix). Null when either address is unparseable.
[KustoImplementation(Keyword = "Functions.Ipv4Compare")]
internal partial class Ipv4CompareFunction
{
    private static long? Impl(string ip1, string ip2) => Ipv4Support.MaskedCompare(ip1, ip2, null);
    private static long? PrefixImpl(string ip1, string ip2, long prefix) => Ipv4Support.MaskedCompare(ip1, ip2, prefix);
}
