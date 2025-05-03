using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;
// ReSharper disable NotAccessedPositionalProperty.Global

namespace Intellisense.FileSystem.Shares;

internal partial class Win32ApiShareReader(
    ILogger<Win32ApiShareReader> logger,
    CancellationTokenSource cts,
    IConnectionVerifier verifier
)
    : IShareReader, IDisposable
{
    private readonly CancellationToken _token = cts.Token;

    public async Task<IEnumerable<string>> GetSharesAsync(string host)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            logger.LogDebug("Share reader is not supported on this {Platform}", RuntimeInformation.OSDescription);
        }

        using var _ = logger.BeginScope(new()
            {
                [nameof(host)] = host
            }
        );

        // win32 NetShareEnum is synchronous and will stall when querying nonexistent addresses
        // so we check first by pinging (which is cancellable)
        // if this takes a long time it probably doesn't exist
        var shouldContinue = await verifier.CanConnectAsync(host);

        if (!shouldContinue)
        {
            return [];
        }


        List<ShareInfo1> shares;

        try
        {
            shares = await AccessResource(host);
        }
        catch (UnauthorizedAccessException e)
        {
            // user might want to know about this? especially with share access being per user account
            // might want to show background information on the frontend
            logger.LogInformation(e, "Access denied while fetching shares.");
            return [];
        }
        catch (FileNotFoundException e)
        {
            // expected when the address is out of network
            logger.LogDebug(e, "Could not find network path.");
            return [];
        }
        catch (Exception e)
        {
            logger.LogWarning(e, "Unexpected exception while fetching shares.");
            throw;
        }

        logger.LogDebug("Fetched host results. {@Hosts}", shares);

        return shares.Select(x => x.Name);
    }
}

internal partial class Win32ApiShareReader
{
    private const int MaxSessions = 2;
    private readonly SemaphoreSlim _semaphore = new(MaxSessions);

    private async Task<List<ShareInfo1>> AccessResource(string host)
    {
        // we can still stall with even when the host exists (firewall? privileges?)
        // we can try ignoring the thread if it doesn't complete within the timeout
        // but this might exhaust the thread pool, so we'll manage usage with a semaphore


        logger.LogTrace("Attempting to acquire lock. {CurrentCount}", _semaphore.CurrentCount);

        try
        {
            await _semaphore.WaitAsync(_token);
        }
        catch (OperationCanceledException e)
        {
            logger.LogTrace(e, "Resource busy. Cannot accept additional requests.");
            throw;
        }

        logger.LogTrace("Acquired lock.");

        // ReSharper disable once MethodSupportsCancellation
        var shareTask = Task.Run(() => GetNetworkSharesAndRelease(host));
        var timeoutTask = _token.Delay();
        var firstTask = await Task.WhenAny(timeoutTask, shareTask);

        if (firstTask == timeoutTask)
        {
            logger.LogDebug("Timed out while fetching shares.");
            return [];
        }

        return await shareTask;
    }

    private List<ShareInfo1> GetNetworkSharesAndRelease(string host)
    {
        logger.LogDebug("Fetching shares from Win32 API for host.");
        try
        {
            return EnumerateShares(host).ToList();
        }
        finally
        {
            var previousCount = _semaphore.Release();
            logger.LogTrace("Released lock. {PreviousLockCount}", previousCount);
        }
    }

    public void Dispose() => _semaphore.Dispose();
}

internal readonly record struct ShareInfo1(string Name = "", string Type = "", string Remark = "");

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal partial class Win32ApiShareReader
{
    private static IEnumerable<ShareInfo1> EnumerateShares(string host) =>
        NetApi32
            .NetShareEnum<NetApi32.SHARE_INFO_1>(host)
            .Select(x =>
                new ShareInfo1(x.shi1_netname.ToString(), x.shi1_type.ToString(), x.shi1_remark?.ToString() ?? "")
            );
}

file static class TokenExtensions
{
    public static async Task Delay(this CancellationToken token, TimeSpan? pollTime = null)
    {
        if (!token.CanBeCanceled)
        {
            return;
        }

        var time = pollTime ?? TimeSpan.FromMilliseconds(100);

        while (!token.IsCancellationRequested)
        {
            // ReSharper disable once MethodSupportsCancellation
#pragma warning disable CA2016
            await Task.Delay(time);
#pragma warning restore CA2016
        }
    }
}
