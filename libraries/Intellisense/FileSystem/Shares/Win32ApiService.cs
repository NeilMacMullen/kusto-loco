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
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // NetShareEnum can potentially block thread for a while, catch most common cause to mitigate thread starvation
            // https://learn.microsoft.com/en-us/windows-server/storage/file-server/smb-ports?tabs=command-line
            // need to take into account different transport protocols and the fact the SMB client can now use port other than 445
            using var ping = new Ping();
            logger.LogDebug("Attempting to connect to host.");
            var pingReply = await ping.SendPingAsync(host, PingTimeout, cancellationToken: cts.Token);
            logger.LogDebug("Connection result: {@PingReply}", pingReply);
            if (pingReply.Status is not IPStatus.Success)
            {
                return [];
            }

            logger.LogDebug("Fetching shares");

            return NetApi32.NetShareEnum<NetApi32.SHARE_INFO_1>(host).Take(MaxItemCount).Select(x => x.shi1_netname);
        }
        finally
        {
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
            .Select(x => pathFactory.Create(x.ui0_remote))
            .OfType<UncPath>()
            .Select(x => x.OriginalHost);
    }
}
