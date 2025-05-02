using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Intellisense.FileSystem;

internal class Win32ApiShareReader(ShareClient client, ILogger<Win32ApiShareReader> logger) : IShareReader
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(250);

    // TODO: async
    public IEnumerable<string> GetShares(string host)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return [];
        }

        using var ping = new Ping();

        try
        {

            var res = ping.Send(host,Timeout.Milliseconds);
            if (res.Status is IPStatus.TimedOut)
            {
                logger.LogInformation("Timed out while pinging host {Host} {@Ping}", host, ping);
            }

            if (res.Status is not IPStatus.Success)
            {
                logger.LogInformation("Failed to ping host. {Host} {@Ping}", host, ping);
                return [];
            }
        }
        catch (TaskCanceledException)
        {
            logger.LogInformation("Timed out while pinging host {Host} {@Ping}", host, ping);
            return [];
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to ping host. {Host} {@Ping}", host, ping);
            return [];
        }

        try
        {
            var shares = client.GetNetworkShares(host);
            return shares.Select(x => x.Name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to fetch shares from Win32 API for host {Host}",host);
            return [];
        }
    }
}
