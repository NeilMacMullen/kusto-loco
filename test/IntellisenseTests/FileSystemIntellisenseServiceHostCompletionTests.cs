using System.Threading.Tasks;
using FluentAssertions;
using Intellisense;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceHostCompletionTests
{
    private readonly IFileSystemIntellisenseService _service;


    public FileSystemIntellisenseServiceHostCompletionTests()
    {
        _service = new ServiceCollection()
            .AddDefault()
            .BuildServiceProvider()
            .GetRequiredService<IFileSystemIntellisenseService>();
    }

    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_AfterValidHostFound_ReturnsHost()
    {
        var twoSlash = "//";
        var localHost = $"//{Constants.LocalHost}/";

        var res1 = await _service.GetPathIntellisenseOptionsAsync(twoSlash);

        res1.Entries.Should().BeEmpty();

        await _service.GetPathIntellisenseOptionsAsync(localHost);

        var res2 = await _service.GetPathIntellisenseOptionsAsync(twoSlash);

        res2
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be(Constants.LocalHost);
    }

    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_AfterValidHostFoundAndPartialName_ReturnsHost()
    {
        var twoSlash = "//";
        var localHost = $"//{Constants.LocalHost}/";
        var localH = $"//localh";

        var res1 = await _service.GetPathIntellisenseOptionsAsync(twoSlash);

        res1.Entries.Should().BeEmpty();

        await _service.GetPathIntellisenseOptionsAsync(localHost);

        var res2 = await _service.GetPathIntellisenseOptionsAsync(localH);

        res2
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be(Constants.LocalHost);
    }
}
