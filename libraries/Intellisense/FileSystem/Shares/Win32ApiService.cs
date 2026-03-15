using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

// TODO: write tests
public class ResourceManager(ILogger<ResourceManager> logger)
{
    private readonly SemaphoreSlim _semaphore = new(2, 2);
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(3);

    public async Task<ResourceLock> AcquireLockAsync(CancellationToken token)
    {
        logger.LogTrace("Waiting for lock");
        var acquired = await _semaphore.WaitAsync(LockTimeout, token);
        if (!acquired)
        {
            throw new TimeoutException("Resource is busy.");
        }

        logger.LogTrace("Acquired lock");

        return new ResourceLock(_semaphore, OnDispose);
    }

    private void OnDispose(int count) => logger.LogTrace("Released lock {Count}", count);
}

public readonly struct ResourceLock(SemaphoreSlim semaphore, Action<int> onDispose) : IDisposable
{
    public void Dispose()
    {
        var count = semaphore.Release();
        onDispose(count);
    }
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class Win32ApiService(
    ILogger<Win32ApiService> logger,
    CancellationTokenSource cts,
        IPathFactory pathFactory,
        ResourceManager resourceManager
    )
        : IShareService
    {
        private const int MaxItemCount = 1000;
        private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan ShareRetrievalWarningTime = TimeSpan.FromSeconds(3);

        // Vanara's NetApi32 methods cause stack overflow on net 10
        // TODO investigate
        private static bool SkipShareOperationsBecauseNet10Broken => false;

        public async Task<IEnumerable<string>> GetSharesAsync(string host)
        {
            if (SkipShareOperationsBecauseNet10Broken)
            {
                logger.LogDebug("Skipping share enumeration in GitHub Actions environment");
                return [];
            }

            if (!await PingHost(host))
        {
            return [];
        }


        logger.LogDebug("Fetching shares");
        using var _ = await resourceManager.AcquireLockAsync(cts.Token);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // NetShareEnum can potentially block thread for a while (~11 seconds) when host exists but port isn't open
            // smb client can use another port besides 445 in newer Windows versions
            // Direct P/Invoke to avoid Vanara stack overflow issue on .NET 10
            var results = NetShareEnumDirect(host)
                .Take(MaxItemCount)
                .ToList();

            logger.LogTrace("Fetched shares: {@Shares}", results);

            return results;
        }
        finally
        {
            stopwatch.Stop();
            logger.LogDebug("{ElapsedTime}", stopwatch.Elapsed);

            if (stopwatch.Elapsed > ShareRetrievalWarningTime)
            {
                logger.LogWarning("Share retrieval took unexpectedly long time {ElapsedTime},{TimeLimit}",
                    stopwatch.Elapsed,
                    ShareRetrievalWarningTime
                );
            }
        }
    }

    private async Task<bool> PingHost(string host)
    {
        // NetShareEnum can potentially block thread for a while (~11 seconds)
        using var ping = new Ping();
        logger.LogDebug("Attempting to connect to host");
        var pingReply = await ping.SendPingAsync(host, PingTimeout, cancellationToken: cts.Token);
        logger.LogDebug("Connection result: {@PingReply}", pingReply);
        return pingReply.Status is IPStatus.Success;
    }

    public async Task<IEnumerable<string>> GetHostsAsync()
    {
        if (SkipShareOperationsBecauseNet10Broken)
        {
            logger.LogDebug("Skipping host enumeration in GitHub Actions environment");
            return [];
        }

        await Task.CompletedTask;

        logger.LogDebug("Fetching hosts");

        // Direct P/Invoke to avoid Vanara stack overflow issue on .NET 10
        var nets = NetUseEnumDirect()
            .Take(MaxItemCount)
            .ToArray();

            return nets
            .Select(remote =>
                {
                    var path = pathFactory.Create(remote);
                    if (path is UncPath p) return p;
                    logger.LogWarning(
                        "Failed to parse host {@FileSystemPath} {Remote}",
                        path,
                        remote
                    );
                    return null;
                }
            )
            .OfType<UncPath>()
            .Select(x => x.OriginalHost);
    }

    private static IEnumerable<string> NetUseEnumDirect()
    {
        const int NERR_Success = 0;
        var result = NetUseEnum(null, 0, out var bufPtr, unchecked((uint)-1), out var entriesRead, out _, out _);
        if (result != NERR_Success)
            yield break;

        try
        {
            var structSize = Marshal.SizeOf<USE_INFO_0>();
            var currentPtr = bufPtr;
            for (var i = 0; i < entriesRead; i++)
            {
                var info = Marshal.PtrToStructure<USE_INFO_0>(currentPtr);
                if (info.ui0_remote != null)
                    yield return info.ui0_remote;
                currentPtr = IntPtr.Add(currentPtr, structSize);
            }
        }
        finally
        {
            NetApiBufferFree(bufPtr);
        }
    }

    private static IEnumerable<string> NetShareEnumDirect(string host)
    {
        const int NERR_Success = 0;
        var result = NetShareEnum(host, 1, out var bufPtr, unchecked((uint)-1), out var entriesRead, out _, out _);
        if (result != NERR_Success)
            yield break;

        try
        {
            var structSize = Marshal.SizeOf<SHARE_INFO_1>();
            var currentPtr = bufPtr;
            for (var i = 0; i < entriesRead; i++)
            {
                var info = Marshal.PtrToStructure<SHARE_INFO_1>(currentPtr);
                if (info.shi1_netname != null)
                    yield return info.shi1_netname;
                currentPtr = IntPtr.Add(currentPtr, structSize);
            }
        }
        finally
        {
            NetApiBufferFree(bufPtr);
        }
    }

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
    private static extern uint NetUseEnum(
        string? serverName,
        uint level,
        out IntPtr bufPtr,
        uint prefMaxLen,
        out uint entriesRead,
        out uint totalEntries,
        out IntPtr resumeHandle);

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode)]
    private static extern uint NetShareEnum(
        string serverName,
        uint level,
        out IntPtr bufPtr,
        uint prefMaxLen,
        out uint entriesRead,
        out uint totalEntries,
        out IntPtr resumeHandle);

    [DllImport("netapi32.dll")]
    private static extern uint NetApiBufferFree(IntPtr buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct USE_INFO_0
    {
        public string? ui0_local;
        public string? ui0_remote;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHARE_INFO_1
    {
        public string? shi1_netname;
        public uint shi1_type;
        public string? shi1_remark;
    }
}
