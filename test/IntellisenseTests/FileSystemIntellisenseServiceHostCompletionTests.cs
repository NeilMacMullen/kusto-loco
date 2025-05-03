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
    public void GetPathIntellisenseOptions_AfterValidHostFound_ReturnsHost()
    {
        var twoSlash = "//";
        var localHost = $"//{Constants.LocalHost}/";

        var res1 = _service.GetPathIntellisenseOptions(twoSlash);

        res1.Entries.Should().BeEmpty();

        _service.GetPathIntellisenseOptions(localHost);

        var res2 = _service.GetPathIntellisenseOptions(twoSlash);

        res2
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be(Constants.LocalHost);
    }
}
