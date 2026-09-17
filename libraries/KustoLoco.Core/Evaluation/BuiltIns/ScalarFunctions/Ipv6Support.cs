//
// Licensed under the MIT License.

using System;
using System.Net;
using System.Net.Sockets;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

// Shared helpers for the ipv6_* family, mirroring Ipv4Support. IPv6 addresses are held as a big-endian UInt128 for
// mask/compare arithmetic, exactly as Ipv4Support holds IPv4 as a uint.
//
// Both address families are accepted, because that is what ADX does: its ipv6_* functions map an IPv4 address into
// IPv6-mapped space (::ffff:a.b.c.d) rather than rejecting it, so `ipv6_lookup` matches an IPv4 source against an
// IPv6 table and vice versa. ParseIpv6Function already models the same rule, including shifting an IPv4 prefix
// length by 96 to account for the 96-bit IPv4-mapped prefix.
internal static class Ipv6Support
{
    /// Parses an IPv4 or IPv6 address to its big-endian UInt128 value, mapping IPv4 into IPv6-mapped space.
    /// <paramref name="wasIpv4"/> reports which family the text was written in, which the caller needs because an
    /// IPv4 prefix length counts from bit 96 of the mapped value.
    public static bool TryParse(string s, out UInt128 value, out bool wasIpv4)
    {
        value = default;
        wasIpv4 = false;
        if (string.IsNullOrEmpty(s) || !IPAddress.TryParse(s, out var ip))
            return false;

        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            wasIpv4 = true;
            ip = ip.MapToIPv6();
        }
        else if (ip.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return false;
        }

        var bytes = ip.GetAddressBytes(); // 16 bytes, network (big-endian) order
        UInt128 v = 0;
        foreach (var b in bytes)
            v = (v << 8) | b;
        value = v;
        return true;
    }

    /// True when <paramref name="ip"/> falls inside <paramref name="cidr"/>. Null when either side is absent or
    /// malformed, matching Ipv4Support.InRange so an unparseable row is a non-match rather than an error.
    public static bool? InRange(string ip, string cidr)
    {
        if (string.IsNullOrEmpty(ip) || string.IsNullOrEmpty(cidr)) return null;

        var slash = cidr.IndexOf('/');
        if (slash < 0) return null;
        if (!int.TryParse(cidr.Substring(slash + 1), out var bits) || bits < 0) return null;

        if (!TryParse(ip, out var ipv, out _)) return null;
        if (!TryParse(cidr.Substring(0, slash), out var basev, out var baseWasIpv4)) return null;

        // An IPv4 CIDR's prefix counts the IPv4 bits, which begin at bit 96 of the IPv4-mapped value.
        if (baseWasIpv4)
        {
            if (bits > 32) return null;
            bits += 96;
        }

        if (bits > 128) return null;

        return (ipv & MaskFor(bits)) == (basev & MaskFor(bits));
    }

    // A /0 must be handled separately: UInt128 shifts mask the count by 127, so `MaxValue << 128` would be MaxValue
    // rather than zero and a /0 would match nothing instead of everything.
    private static UInt128 MaskFor(int bits) =>
        bits == 0 ? UInt128.Zero : UInt128.MaxValue << (128 - bits);
}
