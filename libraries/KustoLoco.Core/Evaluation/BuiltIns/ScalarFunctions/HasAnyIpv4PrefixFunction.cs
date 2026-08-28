using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// has_any_ipv4_prefix(source, ip_prefixes) -> true iff 'source' contains an IPv4 term matching ANY of the prefixes in
// the dynamic array.
[KustoImplementation(Keyword = "Functions.HasAnyIpv4Prefix")]
internal partial class HasAnyIpv4PrefixFunction
{
    private static bool? Impl(string source, JsonNode ipPrefixes) => Ipv4Support.HasAny(source, ipPrefixes, asPrefix: true);
}
