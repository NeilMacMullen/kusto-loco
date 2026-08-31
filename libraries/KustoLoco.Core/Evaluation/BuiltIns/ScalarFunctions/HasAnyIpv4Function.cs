using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// has_any_ipv4(source, ips) -> true iff 'source' contains ANY of the IPv4 addresses in the dynamic array as a term.
[KustoImplementation(Keyword = "Functions.HasAnyIpv4")]
internal partial class HasAnyIpv4Function
{
    private static bool? Impl(string source, JsonNode ips) => Ipv4Support.HasAny(source, ips, asPrefix: false);
}
