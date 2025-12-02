using System;
using System.Net;
using System.Net.Sockets;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "Functions.ParseIPV4")]
internal partial class ParseIpv4Function
{
    private static long? Impl(string ipString)
    {
        if (string.IsNullOrWhiteSpace(ipString))
        {
            return null;
        }

        // Check if there's a CIDR prefix
        int prefixLength = 32;
        var slashIndex = ipString.IndexOf('/');
        if (slashIndex >= 0)
        {
            var prefixPart = ipString.Substring(slashIndex + 1);
            if (!int.TryParse(prefixPart, out prefixLength) || prefixLength < 0 || prefixLength > 32)
            {
                return null;
            }
            ipString = ipString.Substring(0, slashIndex);
        }

        if (!IPAddress.TryParse(ipString, out var ip))
        {
            return null;
        }

        if (ip.AddressFamily != AddressFamily.InterNetwork)
        {
            return null;
        }

        var bytes = ip.GetAddressBytes();
        // Convert to big-endian long (network byte order)
        long result = ((long)bytes[0] << 24) | ((long)bytes[1] << 16) | ((long)bytes[2] << 8) | bytes[3];

        // Apply the netmask if prefix length is less than 32
        if (prefixLength < 32)
        {
            // Create a mask with prefixLength bits set from the left
            long mask = prefixLength == 0 ? 0 : ~((1L << (32 - prefixLength)) - 1) & 0xFFFFFFFFL;
            result &= mask;
        }

        return result;
    }
}
