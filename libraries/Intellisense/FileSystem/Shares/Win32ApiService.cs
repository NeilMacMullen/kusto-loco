using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
internal class Win32ApiService(
    ILogger<Win32ApiService> logger,
    CancellationTokenSource cts,
    IPathFactory pathFactory
)
    : IShareService
{
    private const int MaxItemCount = 1000;
    private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(5);

    public async Task<IEnumerable<string>> GetSharesAsync(string host)
    {
        // NetShareEnum can potentially block thread for a while (~11 seconds), may want to run this on another thread
        // thread pool exhaustion probably won't be an issue with how often this would be invoked
        // can occur if device exists but TCP port 445 isn't open
        // https://learn.microsoft.com/en-us/windows-server/storage/file-server/smb-ports?tabs=command-line
        // we may want to support other protocols + alternative TCP port (useful for docker testing)
        // so only checking for host existence rather than port 445 availability
        // we can't just use stored connections because they aren't exhaustive (i.e. IP address aliases) and we want to allow user to list shares for a given host even if we missed it
        // we shouldn't use our own SMB client implementation because we'd have to obtain and manage credentials ourselves (which the OS implementation does for us)
        using var ping = new Ping();
        logger.LogDebug("Attempting to connect to host.");
        var pingReply = await ping.SendPingAsync(host, PingTimeout, cancellationToken: cts.Token);
        logger.LogDebug("Connection result: {@PingReply}", pingReply);
        if (pingReply.Status is not IPStatus.Success)
        {
            return [];
        }

        logger.LogDebug("Fetching shares");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var results = NetApi32
                .NetShareEnum<NetApi32.SHARE_INFO_1>(host)
                .Take(MaxItemCount)
                .Select(x => x.shi1_netname)
                .ToList();

            logger.LogTrace("Fetched shares: {@Shares}", results);

            return results;
        }
        finally
        {
            stopwatch.Stop();
            logger.LogDebug("{ElapsedTime}", stopwatch.Elapsed);
            var timeLimit = TimeSpan.FromSeconds(3);
            if (stopwatch.Elapsed > timeLimit)
            {
                logger.LogWarning("Share retrieval took unexpectedly long time {ElapsedTime},{TimeLimit}",
                    stopwatch.Elapsed,
                    timeLimit
                );
            }
        }
    }

    public async Task<IEnumerable<string>> GetHostsAsync()
    {
        await Task.CompletedTask;

        logger.LogDebug("Fetching hosts");

        return NetApi32
            .NetUseEnum<NetApi32.USE_INFO_0>()
            .Take(MaxItemCount)
            .Select(useInfo0 =>
                {
                    var remote = useInfo0.ui0_remote;
                    var path = pathFactory.Create(remote);
                    if (path is UncPath p) return p;
                    logger.LogWarning(
                        "Failed to parse host from unexpected remote format {@FileSystemPath} {PathInfo} {Remote}",
                        path,
                        useInfo0,
                        remote
                    );
                    return null;
                }
            )
            .OfType<UncPath>()
            .Select(x => x.OriginalHost);
    }
}
