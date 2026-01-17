using System.Net;
using System.Runtime.InteropServices;
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
    private IFileSystemIntellisenseService? _service;

    private static bool ShouldSkip() =>
        !PlatformHelper.IsCi() ||
        !RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
        !PlatformHelper.IsWindowsAdmin();

    public async Task InitializeAsync()
    {
        // Skip initialization if tests will be skipped anyway
        // This prevents stack overflow from Win32 API calls in unsupported environments
        if (ShouldSkip())
        {
            return;
        }

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

    public async Task DisposeAsync()
    {
        // Only cleanup if we actually initialized
        if (_service is null)
        {
            return;
        }

        await RunAsync("net", $@"use \\{IPAddress.Loopback} /delete", noEcho: true);
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_TwoSlash_ShowsServersFromPersistedShareConnections()
    {
        var result = await _service!.GetPathIntellisenseOptionsAsync("//");
        result.Entries.Should().ContainSingle(x => x.Name == IPAddress.Loopback.ToString());
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_PartialHost_ShowsServersFromPersistedShareConnectionsWithFilter()
    {
        var result = await _service!.GetPathIntellisenseOptionsAsync("//172.");
        result.Entries.Should().ContainSingle(x => x.Name == IPAddress.Loopback.ToString());
        result.Filter.Should().Be("172.");
    }


}
