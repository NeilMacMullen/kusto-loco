using Microsoft.Extensions.Logging;
using NotNullStrings;
using SimpleExec;

namespace Intellisense.FileSystem;

internal interface IShareReader
{
    public IEnumerable<string> GetShares(string host);
}

internal class ShareReader(ILogger<ShareReader> logger) : IShareReader
{
    private static readonly TimeSpan ShareReaderTimeout = TimeSpan.FromMilliseconds(250); // TODO: move to configs
    public IEnumerable<string> GetShares(string host)
    {
        try
        {
            var cts = new CancellationTokenSource(ShareReaderTimeout);
            var res = Command.ReadAsync("net", $"view {host} /all", cancellationToken: cts.Token).Result;
            if (res.StandardError.IsNotBlank())
            {
                logger.LogWarning("Unexpected output from net view command: {Output}", res.StandardError);
            }

            return NetViewShareRow.Parse(res.StandardOutput).Select(x => x.ShareName);
        }
        catch (Exception e)
        {
            if (e is not AggregateException {InnerExceptions: [{} inner]})
            {
                throw;
            }

            if (inner.Message.Contains("The network path was not found."))
            {
                logger.LogInformation(
                    "Failed to find network path while fetching shares for {Host}; it likely does not exist.",
                    host
                );
                return [];
            }

            if (inner is TaskCanceledException)
            {
                logger.LogInformation("Timed out while fetching shares for {Host}; network path likely does not exist.", host);
                return [];

            }

            throw;
        }

    }
}
