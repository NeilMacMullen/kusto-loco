using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Configuration;
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
    private static readonly TimeSpan ShareRetrievalWarningTime = TimeSpan.FromSeconds(3);

    public async Task<IEnumerable<string>> GetSharesAsync(string host)
    {

        if (!await PingHost(host))
        {
            return [];
        }

        logger.LogDebug("Fetching shares");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            // NetShareEnum can potentially block thread for a while (~11 seconds) when host exists but port isn't open
            // smb client can use another port besides 445 in newer Windows versions
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
                        "Failed to parse host {@FileSystemPath} {PathInfo} {Remote}",
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
