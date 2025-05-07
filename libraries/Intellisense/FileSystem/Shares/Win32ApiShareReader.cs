using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intellisense.FileSystem.Shares;

internal class Win32ApiShareReader(
    ILogger<Win32ApiShareReader> logger,
    IHostRepository hostRepository,
    IOptions<IntellisenseTimeoutOptions> options,
    IShareResource resource,
    CancellationTokenSource cts
    )
    : IShareReader
{
    public async Task<IEnumerable<string>> GetSharesAsync(string host)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            logger.LogDebug("Not running on Windows. Skipping.");
            return [];
        }

        using var _ = logger.BeginScope(new Dictionary<string, object>
            {
                [nameof(UriHostNameType)] = Uri.CheckHostName(host),
                [nameof(host)] = host
            }
        );

        // win32 NetShareEnum is synchronous and will stall when querying nonexistent addresses
        // so we check first by pinging (which is cancellable)
        // if this takes a long time it probably doesn't exist
        var shouldContinue = await PingHostAsync(host);

        if (!shouldContinue)
        {
            return [];
        }

        // populate host autocompletion if it's a valid host
        await hostRepository.AddAsync(host);

        var res = await resource.GetSharesAsync(host, cts.Token);
        return res.Select(x => x.Name);
    }

    private async Task<bool> PingHostAsync(string host)
    {

        using var ping = new Ping();

        try
        {
            // most importantly, we want a cts for cancelling DNS resolution
            // https://github.com/dotnet/runtime/blob/0654416af35b729fd3620b2dc41208a22fc8b977/src/libraries/System.Net.Ping/src/System/Net/NetworkInformation/Ping.cs#L719-L725
            var pingReply = await ping.SendPingAsync(host, options.Value.IntellisenseTimeout, cancellationToken: cts.Token);
            using var __ = logger.BeginScope(new()
            {
                [$"@{nameof(PingReply)}"] = pingReply
            });
            if (pingReply.Status is IPStatus.TimedOut)
            {
                logger.LogTrace("Timed out during ICMP request");
                return false;
            }

            if (pingReply.Status is not IPStatus.Success)
            {
                logger.LogTrace("Failed ICMP request");
                return false;
            }

            logger.LogTrace("Confirmed host exists.");

            return true;
        }
        catch (Exception e) when (e is TaskCanceledException or OperationCanceledException)
        {
            logger.LogTrace("Timed out while pinging.");
            return false;
        }
        catch (Exception e)
        {
            logger.LogTrace(e, "Failed to ping host.");
            return false;
        }
    }
}
