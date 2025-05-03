using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

internal interface IHostReader
{
    public Task<IEnumerable<string>> GetHostsAsync();
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class Win32ApiHostReader(IPathFactory pathFactory, ILogger<Win32ApiHostReader> logger) : IHostReader
{
    public async Task<IEnumerable<string>> GetHostsAsync()
    {
        await Task.CompletedTask;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            logger.LogDebug("Share reader is not supported on this {Platform}", RuntimeInformation.OSDescription);
        }

        var serverNamesOfRememberedSmbConnections = NetApi32
            .NetUseEnum<NetApi32.USE_INFO_0>()
            .Select(x => pathFactory.Create(x.ui0_remote))
            .OfType<UncPath>()
            .Select(x => x.Host);


        // TODO: we need to filter out devices that aren't serving shares but also without doing expensive checks (problem with NetApi32.NetShareEnum)
        // otherwise the completions are going to be flooded with useless results
        // var ipAddressesInLocalNetwork = GetLocalNetworkDevices().Select(x => x.ToString());

        // we'll just hard code share access on the local device
        // the aliases for localhost aren't important for the use case probably
        var results = serverNamesOfRememberedSmbConnections.Append("localhost").Distinct().ToList();

        logger.LogDebug("Found hosts. {@Hosts}", results);

        return results;
    }

    // private static IEnumerable<IPAddress> GetLocalNetworkDevices()
    // {
    //     // do unc paths even work with ipv6? let's just do ipv4 for now
    //     var result = IpHlpApi.GetIpNetTable2(Ws2_32.ADDRESS_FAMILY.AF_INET, out var table);
    //
    //     result.ThrowIfFailed();
    //
    //     return GetIpAddresses();
    //
    //     IEnumerable<IPAddress> GetIpAddresses()
    //     {
    //         return table
    //             .Select(x => x.Address.Ipv4.sin_addr.S_un_b)
    //             .Select(x => new IPAddress(x));
    //     }
    // }


}