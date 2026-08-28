using System;
using System.Net;
using System.Net.Sockets;

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
