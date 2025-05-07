using Microsoft.Extensions.Logging;

namespace Intellisense.FileSystem.Shares;

internal class Win32ShareResource(ILogger<Win32ShareResource> logger, IShareClient client) : IShareResource
{
    private readonly SemaphoreSlim _semaphore = new(2,2);

    public async Task<List<ShareInfo>> GetSharesAsync(string host, CancellationToken token)
    {
        // we can still stall with even when the host exists (firewall? privileges?)
        // we can try ignoring the thread if it doesn't complete within the timeout
        // but this might exhaust the thread pool, so we'll manage usage with a semaphore



        logger.LogTrace("Attempting to acquire lock. {CurrentCount}", _semaphore.CurrentCount);

        try
        {
            await _semaphore.WaitAsync(token);
        }
        catch (OperationCanceledException e)
        {
            logger.LogTrace(e,"Resource busy. Cannot accept additional requests.");
            throw;
        }

        logger.LogTrace("Acquired lock.");

        // ReSharper disable once MethodSupportsCancellation
        var shareTask = Task.Run(() => GetNetworkSharesAndRelease(host));
        var timeoutTask = token.Delay();
        var firstTask = await Task.WhenAny(timeoutTask, shareTask);

        if (firstTask == timeoutTask)
        {
            logger.LogDebug("Timed out while fetching shares.");
            return [];
        }

        return await shareTask;
    }

    private List<ShareInfo> GetNetworkSharesAndRelease(string host)
    {
        logger.LogDebug("Fetching shares from Win32 API for host.");
        try
        {
            return client.GetNetworkShares(host).ToList();
        }
        catch (UnauthorizedAccessException e)
        {
            logger.LogWarning(e, "Access denied while fetching shares.");
            throw;
        }
        catch (FileNotFoundException e)
        {
            logger.LogDebug(e, "Could not find network path.");
            return [];
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unexpected exception while fetching shares.");
            throw;
        }
        finally
        {
            var previousCount = _semaphore.Release();
            logger.LogTrace("Released lock. {PreviousLockCount}", previousCount);
        }
    }

    public void Dispose() => _semaphore.Dispose();
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
