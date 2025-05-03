using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Intellisense.FileSystem;

internal class Win32ApiShareReader(ShareClient client, ILogger<Win32ApiShareReader> logger, IMemoryCache cache)
    : IShareReader
{
    // TODO: configs?

    // if this takes a long time it probably doesn't exist
    private static readonly TimeSpan ShareEnumerationTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan IcmpTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan PingTimeout = IcmpTimeout.Add(TimeSpan.FromMilliseconds(100));
    private static readonly TimeSpan SemaphoreTimeout = ShareEnumerationTimeout.Add(PingTimeout);
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    private readonly SemaphoreSlim _semaphore = new(2, 2);

    // TODO: full async refactor
    public IEnumerable<string> GetShares(string host)
    {
        if (cache.Get<List<ShareInfo>>(host) is { } val)
        {
            logger.LogInformation("Found cached {@Shares} for {Host}", val, host);
            return val.Select(x => x.Name);
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return [];
        }

        using var _ = logger.BeginScope(new Dictionary<string, object>
            {
                [nameof(UriHostNameType)] = Uri.CheckHostName(host)
            }
        );

        // win32 NetShareEnum is synchronous and will stall when querying nonexistent addresses
        // so we check first by pinging (which is cancellable)
        var shouldContinue = Task.Run(async () => await PingHostAsync(host)).GetAwaiter().GetResult();

        if (!shouldContinue)
        {
            if (cache.Get<List<ShareInfo>>(host) is {Count: > 0})
            {
                logger.LogDebug("{Host} previously contained results, refusing to override cache",host);
                return [];
            }
            logger.LogDebug(
                "Adding {Host} to cache with expiration {Expiration} and empty entries because it failed to ping",
                host,
                CacheExpiration
            );
            cache.Set(host, new List<ShareInfo>(), CacheExpiration);
            return [];
        }


        try
        {
            var res = Task.Run(async () => await GetSharesAsync(host)).GetAwaiter().GetResult();
            cache.Set(host, res, CacheExpiration);
            logger.LogDebug("Cached {@Shares} for {Host} with expiration {Expiration}", res, host, CacheExpiration);
            return res.Select(x => x.Name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to fetch shares from Win32 API for host {Host}", host);
            return [];
        }
    }

    private async Task<List<ShareInfo>> GetSharesAsync(string host)
    {
        // we can still fail (firewall? permissions?)
        // so instead we will just ignore the result after a given amount of time instead of cancelling it
        // but this might exhaust the thread pool, so we'll manage with a semaphore

        logger.LogDebug("Attempting to acquire lock. {CurrentCount}", _semaphore.CurrentCount);
        if (!await _semaphore.WaitAsync(SemaphoreTimeout))
        {
            logger.LogDebug("Thread pool is busy. Skipping share enumeration for host {Host}", host);
            return [];
        }

        logger.LogDebug("Acquired lock. {RemainingCount}", _semaphore.CurrentCount);

        var shareTask = Task.Factory.StartNew(() => GetNetworkSharesImpl(host), TaskCreationOptions.LongRunning);
        var timeoutTask = Task.Delay(ShareEnumerationTimeout);
        var firstTask = await Task.WhenAny(timeoutTask, shareTask);
        if (firstTask == timeoutTask)
        {
            logger.LogError("Timed out after {Time} while fetching shares from Win32 API for host {Host}",
                ShareEnumerationTimeout,
                host
            );
            return [];
        }

        return await shareTask;
    }

    private List<ShareInfo> GetNetworkSharesImpl(string host)
    {
        logger.LogDebug("Fetching shares from Win32 API for host {Host}", host);
        try
        {
            return client.GetNetworkShares(host).ToList();
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to fetch shares from Win32 API for host {Host}", host);
            return [];
        }
        finally
        {
            _semaphore.Release();
            logger.LogDebug("Released lock. {CurrentLockCount}", _semaphore.CurrentCount);
        }
    }


    private async Task<bool> PingHostAsync(string host)
    {
        using var cts = new CancellationTokenSource(PingTimeout);
        using var ping = new Ping();

        try
        {
            // most importantly, we want a cts for cancelling DNS resolution
            // https://github.com/dotnet/runtime/blob/0654416af35b729fd3620b2dc41208a22fc8b977/src/libraries/System.Net.Ping/src/System/Net/NetworkInformation/Ping.cs#L719-L725
            var pingReply = await ping.SendPingAsync(host, IcmpTimeout, cancellationToken: cts.Token);
            if (pingReply.Status is IPStatus.TimedOut)
            {
                logger.LogInformation("Timed out after {Time} during ICMP request {Host} {@PingReply}",
                    IcmpTimeout,
                    host,
                    pingReply
                );
                return false;
            }

            if (pingReply.Status is not IPStatus.Success)
            {
                logger.LogWarning("Failed ICMP request {Host} {@PingReply}", host, pingReply);
                return false;
            }

            logger.LogInformation("Found valid host {Host} {@PingReply}", host, pingReply);

            return true;
        }
        catch (Exception e) when (e is TaskCanceledException or OperationCanceledException)
        {
            logger.LogInformation("Timed out after {Time} while pinging {Host}", IcmpTimeout, host);
            return false;
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Failed to ping host. {Host}", host);
            return false;
        }
    }
}
