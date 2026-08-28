using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// Shared helpers for the ipv4_* family. A separate class because the source generator INLINES an Impl body, so an Impl
// must not call a private same-class helper. IPv4 addresses are held as a big-endian uint for mask/compare arithmetic.
internal static class Ipv4Support
{
    public static bool TryParse(string s, out uint value)
    {
        value = 0;
        if (!IPAddress.TryParse(s, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork) return false;
        var b = ip.GetAddressBytes();
        value = ((uint)b[0] << 24) | ((uint)b[1] << 16) | ((uint)b[2] << 8) | b[3];
        return true;
    }

    // Split "a.b.c.d[/prefix]" into address + prefix (32 when no suffix; a malformed suffix -> 32).
    public static int SplitPrefix(string token, out string ip)
    {
        var slash = token.IndexOf('/');
        if (slash < 0) { ip = token; return 32; }
        ip = token.Substring(0, slash);
        return int.TryParse(token.Substring(slash + 1), out var p) && p >= 0 && p <= 32 ? p : 32;
    }

    public static bool? InRange(string ip, string cidr)
    {
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(cidr)) return null;
        var slash = cidr.IndexOf('/');
        if (slash < 0) return null;
        if (!TryParse(ip, out var ipv) || !TryParse(cidr.Substring(0, slash), out var basev)) return null;
        if (!int.TryParse(cidr.Substring(slash + 1), out var bits) || bits < 0 || bits > 32) return null;
        var mask = bits == 0 ? 0u : 0xFFFFFFFFu << (32 - bits);
        return (ipv & mask) == (basev & mask);
    }

    public static string UintToDotted(uint v) =>
        $"{(v >> 24) & 0xFF}.{(v >> 16) & 0xFF}.{(v >> 8) & 0xFF}.{v & 0xFF}";

    // The network address of 'ip' masked to the smaller of its own /suffix and 'prefix' (default 32), as a uint.
    // Null when the address is unparseable or the effective prefix is out of range.
    public static uint? MaskedValue(string ip, long? prefix)
    {
        if (string.IsNullOrEmpty(ip)) return null;
        var p = SplitPrefix(ip, out var addr);
        if (!TryParse(addr, out var v)) return null;
        var bits = (int)System.Math.Min(p, prefix ?? 32);
        if (bits < 0 || bits > 32) return null;
        var mask = bits == 0 ? 0u : 0xFFFFFFFFu << (32 - bits);
        return v & mask;
    }

    // format_ipv4: the masked network address as dotted-quad.
    public static string? FormatV4(string ip, long? prefix)
    {
        var v = MaskedValue(ip, prefix);
        return v is null ? null : UintToDotted(v.Value);
    }

    // format_ipv4_mask: the masked network address in CIDR notation, using the effective prefix.
    public static string? FormatV4Mask(string ip, long prefix)
    {
        if (prefix < 0 || prefix > 32) return null;
        var p = SplitPrefix(ip, out _);
        var bits = System.Math.Min(p, (int)prefix);
        var v = MaskedValue(ip, prefix);
        return v is null ? null : $"{UintToDotted(v.Value)}/{bits}";
    }

    // ipv4_netmask_suffix: the /suffix of a CIDR (32 when none present); null when the address is unparseable.
    public static long? NetmaskSuffix(string ipRange)
    {
        if (string.IsNullOrEmpty(ipRange)) return null;
        var p = SplitPrefix(ipRange, out var addr);
        return TryParse(addr, out _) ? p : null;
    }

    // parse_ipv4_mask: the masked network address as a long.
    public static long? ParseV4Mask(string ip, long prefix)
    {
        var v = MaskedValue(ip, prefix);
        return v is null ? null : (long)v.Value;
    }

    // ipv4_range_to_cidr_list: the minimal ordered list of CIDR blocks that exactly covers [start_ip, end_ip].
    public static JsonNode? RangeToCidrList(string startIp, string endIp)
    {
        if (!TryParse(startIp, out var start) || !TryParse(endIp, out var end) || start > end) return null;
        var result = new JsonArray();
        ulong s = start;
        ulong e = end;
        while (s <= e)
        {
            // Largest block the current start is aligned for, capped by how many addresses remain.
            var maxByAlign = s == 0 ? 32 : BitOperations.TrailingZeroCount((uint)s);
            var maxByCount = BitOperations.Log2(e - s + 1);
            var blockBits = (int)Math.Min(maxByAlign, maxByCount);
            result.Add($"{UintToDotted((uint)s)}/{32 - blockBits}");
            s += 1UL << blockBits;
        }
        return result;
    }

    // has_ipv4 / has_ipv4_prefix scan for IPv4 terms delimited by non-alphanumeric, non-dot characters (so a substring
    // of a longer number or word is not a match), matching ADX term semantics.
    private static readonly Regex Ipv4Term =
        new(@"(?<![0-9A-Za-z.])(\d{1,3}\.){3}\d{1,3}(?![0-9A-Za-z.])", RegexOptions.CultureInvariant);

    public static bool? HasIp(string? source, string ip)
    {
        if (source is null) return null;
        if (!TryParse(ip, out var target)) return null;
        foreach (Match m in Ipv4Term.Matches(source))
            if (TryParse(m.Value, out var v) && v == target) return true;
        return false;
    }

    public static bool? HasIpPrefix(string? source, string prefix)
    {
        if (source is null) return null;
        var parts = (prefix ?? "").TrimEnd('.').Split('.');
        if (parts.Length is 0 or > 4) return null;
        var octets = new int[parts.Length];
        for (var i = 0; i < parts.Length; i++)
            if (!int.TryParse(parts[i], out octets[i]) || octets[i] < 0 || octets[i] > 255) return null;
        foreach (Match m in Ipv4Term.Matches(source))
        {
            var cand = m.Value.Split('.');
            if (cand.Length != 4) continue;
            var ok = true;
            for (var i = 0; i < octets.Length; i++)
                if (!int.TryParse(cand[i], out var o) || o != octets[i]) { ok = false; break; }
            if (ok) return true;
        }
        return false;
    }

    public static bool? HasAny(string? source, JsonNode needles, bool asPrefix)
    {
        if (source is null || needles is not JsonArray arr) return null;
        foreach (var item in arr)
        {
            var needle = item?.ToString();
            if (string.IsNullOrEmpty(needle)) continue;
            var hit = asPrefix ? HasIpPrefix(source, needle) : HasIp(source, needle);
            if (hit == true) return true;
        }
        return false;
    }

    public static bool? MaskedEqual(string a, string b, long? prefixArg)
    {
        var m = Masked(a, b, prefixArg, out var av, out var bv);
        return m is null ? null : av == bv;
    }

    public static long? MaskedCompare(string a, string b, long? prefixArg)
    {
        var m = Masked(a, b, prefixArg, out var av, out var bv);
        return m is null ? null : (av == bv ? 0L : av < bv ? -1L : 1L);
    }

    // Parse both operands (each may carry its own /prefix), mask to the smallest of the two suffixes and any explicit
    // prefix argument, and hand back the masked values. Null when either operand is unparseable.
    private static bool? Masked(string a, string b, long? prefixArg, out uint av, out uint bv)
    {
        av = 0; bv = 0;
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return null;
        var pa = SplitPrefix(a, out var aIp);
        var pb = SplitPrefix(b, out var bIp);
        if (!TryParse(aIp, out var a0) || !TryParse(bIp, out var b0)) return null;
        var bits = Math.Min(Math.Min(pa, pb), (int)(prefixArg ?? 32));
        if (bits < 0 || bits > 32) return null;
        var mask = bits == 0 ? 0u : 0xFFFFFFFFu << (32 - bits);
        av = a0 & mask; bv = b0 & mask;
        return true;
    }
}
