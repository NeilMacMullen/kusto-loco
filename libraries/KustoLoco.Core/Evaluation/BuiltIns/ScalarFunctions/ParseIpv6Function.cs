using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ParseIPV6")]
internal partial class ParseIpv6Function
{
    private static string Impl(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
        {
            return string.Empty;
        }

        // Check if there's a CIDR prefix
        int prefixLength = -1;
        var slashIndex = ipString.IndexOf('/');
        if (slashIndex >= 0)
        {
            var prefixPart = ipString.Substring(slashIndex + 1);
            if (!int.TryParse(prefixPart, out prefixLength))
            {
                return string.Empty;
            }
            ipString = ipString.Substring(0, slashIndex);
        }

        if (!IPAddress.TryParse(ipString, out var ip))
        {
            return string.Empty;
        }

        // Convert IPv4 to IPv6-mapped format if needed
        if (ip.AddressFamily == AddressFamily.InterNetwork)
        {
            ip = ip.MapToIPv6();
            // For IPv4, prefix lengths 0-32 need to be adjusted to IPv6 equivalent (add 96)
            if (prefixLength >= 0)
            {
                if (prefixLength > 32)
                {
                    return string.Empty;
                }
                prefixLength += 96;
            }
        }
        else if (ip.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return string.Empty;
        }

        // Validate prefix length for IPv6
        if (prefixLength >= 0 && (prefixLength < 0 || prefixLength > 128))
        {
            return string.Empty;
        }

        var bytes = ip.GetAddressBytes();

        // Apply the netmask if prefix length is specified (inline logic)
        if (prefixLength >= 0 && prefixLength < 128)
        {
            int fullBytes = prefixLength / 8;
            int remainingBits = prefixLength % 8;

            // Set all bytes after the prefix to 0
            for (int i = fullBytes + (remainingBits > 0 ? 1 : 0); i < bytes.Length; i++)
            {
                bytes[i] = 0;
            }

            // Apply partial mask to the byte at the boundary
            if (remainingBits > 0 && fullBytes < bytes.Length)
            {
                byte mask = (byte)(0xFF << (8 - remainingBits));
                bytes[fullBytes] &= mask;
            }
        }

        // Format as canonical IPv6 with all 8 groups of 4 hex digits
        var sb = new StringBuilder(39);
        for (int i = 0; i < 16; i += 2)
        {
            if (i > 0)
            {
                sb.Append(':');
            }
            sb.Append(bytes[i].ToString("x2"));
            sb.Append(bytes[i + 1].ToString("x2"));
        }

        return sb.ToString();
    }
}
