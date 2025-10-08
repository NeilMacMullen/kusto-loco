using System;
using System.Net;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl;

[KustoImplementation(Keyword = "ipv4_is_private")]
internal partial class Ipv4IsPrivateFunction
{
    private static bool Impl(string ipstring)
    {
        if (!IPAddress.TryParse(ipstring, out IPAddress? ip))
        {
            return false;
        }

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            byte[] bytes = ip.GetAddressBytes();

            // 10.0.0.0/8
            if (bytes[0] == 10)
                return true;

            // 172.16.0.0/12
            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                return true;

            // 192.168.0.0/16
            if (bytes[0] == 192 && bytes[1] == 168)
                return true;
        }

        return false;
    }
}
