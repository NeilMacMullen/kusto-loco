using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;
using static SimpleExec.Command;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceHostCompletionTests
{
    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_UncPaths_ShowsServersFromPersistedShareConnections()
    {
        var loopback = IPAddress.Loopback;
        using var share = TestShare.Create();
        var provider = new ServiceCollection().AddDefault().BuildServiceProvider();
        var service = provider.GetRequiredService<IFileSystemIntellisenseService>();


        var result1 = await service.GetPathIntellisenseOptionsAsync("//");
        result1
            .Entries.Should()
            .NotContain(x => x.Name == loopback.ToString(),
                "Error: Cannot run test when loopback address is already in use for SMB share. Disable this test if you need it for some other reason, otherwise remove the connection."
            );


        try
        {
            await RunAsync("net", $@"use \\{loopback}", noEcho: true);
            var result = await service.GetPathIntellisenseOptionsAsync("//");
            result.Entries.Should().ContainSingle(x => x.Name == loopback.ToString());
        }
        finally
        {
            await RunAsync("net", $@"use \\{loopback} /delete", noEcho: true);
        }
    }

    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_UncPaths_ShowsLocalhost()
    {
        var provider = new ServiceCollection().AddDefault().BuildServiceProvider();
        var service = provider.GetRequiredService<IFileSystemIntellisenseService>();


        var result1 = await service.GetPathIntellisenseOptionsAsync("//");
        result1.Entries.Should().ContainSingle(x => x.Name == Constants.LocalHost);
    }
}
