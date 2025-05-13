using System.Runtime.InteropServices;
using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging;
using Vanara.PInvoke;

namespace Intellisense.FileSystem.Shares;

internal interface IHostReader
{
    public Task<IEnumerable<string>> GetHostsAsync();
}


internal class Win32ApiHostReader(IPathFactory pathFactory, ILogger<Win32ApiHostReader> logger) : IHostReader
{
    public async Task<IEnumerable<string>> GetHostsAsync()
    {
        await Task.CompletedTask;
        if (!OperatingSystem.IsWindows())
        {
            logger.LogDebug("Reader is not supported on this {Platform}", RuntimeInformation.OSDescription);
            return [];
        }

        var serverNamesOfRememberedSmbConnections = new List<string>();

        foreach (var item in NetApi32.NetUseEnum<NetApi32.USE_INFO_0>())
        {
            var path = pathFactory.Create(item.ui0_remote);
            if (path is UncPath p)
            {
                serverNamesOfRememberedSmbConnections.Add(p.Host);
            }
        }

        // the aliases aren't important
        var results = serverNamesOfRememberedSmbConnections
            .Append("localhost").Distinct().ToList();

        logger.LogTrace("Fetched {@Results}",results);


        return results;
    }
}