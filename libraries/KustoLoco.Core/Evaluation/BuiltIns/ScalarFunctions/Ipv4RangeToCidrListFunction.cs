using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ipv4_range_to_cidr_list(start_ip, end_ip) -> the minimal ordered array of CIDR blocks covering the inclusive range;
// null when either address is unparseable or start > end.
[KustoImplementation(Keyword = "Functions.Ipv4RangeToCidrList")]
internal partial class Ipv4RangeToCidrListFunction
{
    private static JsonNode? Impl(string startIp, string endIp) => Ipv4Support.RangeToCidrList(startIp, endIp);
}
