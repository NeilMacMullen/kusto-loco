using System.Text.Json.Nodes;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// ipv4_is_in_any_range(ip, ranges) -> true iff the IPv4 'ip' is inside ANY of the CIDR ranges in the dynamic array;
// false if in none; null when 'ip' is unparseable or 'ranges' is not a dynamic array.
[KustoImplementation(Keyword = "Functions.Ipv4IsInAnyRange")]
internal partial class Ipv4IsInAnyRangeFunction
{
    private static bool? Impl(string ip, JsonNode ranges)
    {
        if (string.IsNullOrEmpty(ip) || ranges is not JsonArray arr) return null;
        if (!Ipv4Support.TryParse(ip, out var ipv)) return null;
        foreach (var item in arr)
        {
            var cidr = item?.ToString();
            if (string.IsNullOrEmpty(cidr)) continue;
            uint basev;
            int bits;
            var slash = cidr.IndexOf('/');
            if (slash < 0)
            {
                // a bare address in the list is an exact (/32) match, as in ADX.
                if (!Ipv4Support.TryParse(cidr, out basev)) continue;
                bits = 32;
            }
            else
            {
                if (!Ipv4Support.TryParse(cidr.Substring(0, slash), out basev)) continue;
                if (!int.TryParse(cidr.Substring(slash + 1), out bits) || bits < 0 || bits > 32) continue;
            }
            var mask = bits == 0 ? 0u : 0xFFFFFFFFu << (32 - bits);
            if ((ipv & mask) == (basev & mask)) return true;
        }
        return false;
    }
}
