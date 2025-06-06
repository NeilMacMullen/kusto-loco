using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static SimpleExec.Command;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceHostCompletionTests : IAsyncLifetime
{
    private IFileSystemIntellisenseService _service = null!;

    public async Task InitializeAsync()
    {
        var provider = new IntegrationTestContainer();
        var service = provider.GetRequiredService<IFileSystemIntellisenseService>();
        var result = await service.GetPathIntellisenseOptionsAsync("//");

        result
            .Entries.Should()
            .NotContain(x => x.Name == IPAddress.Loopback.ToString(),
                "Error: Cannot run test when loopback address is already in use for SMB share."
            );
        await RunAsync("net", $@"use \\{IPAddress.Loopback}", noEcho: true);
        _service = service;
    }

    public async Task DisposeAsync() => await RunAsync("net", $@"use \\{IPAddress.Loopback} /delete", noEcho: true);

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_TwoSlash_ShowsServersFromPersistedShareConnections()
    {
        var result = await _service.GetPathIntellisenseOptionsAsync("//");
        result.Entries.Should().ContainSingle(x => x.Name == IPAddress.Loopback.ToString());
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_PartialHost_ShowsServersFromPersistedShareConnectionsWithFilter()
    {
        var result = await _service.GetPathIntellisenseOptionsAsync("//172.");
        result.Entries.Should().ContainSingle(x => x.Name == IPAddress.Loopback.ToString());
        result.Filter.Should().Be("172.");
    }


}
