using System.IO;
using System.Linq;
using FluentAssertions;
using Intellisense;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceShareTests
{
    private readonly IFileSystemIntellisenseService _service =
        new ServiceCollection()
            .AddDefault()
            .BuildServiceProvider()
            .GetRequiredService<IFileSystemIntellisenseService>();

    [WindowsOnlyFact]
    public void GetPathIntellisenseOptions_ChildDir_RetrievesChildrenOfChildDir()
    {
        var dir = Directory.CreateDirectory($"//{Constants.LocalHost}/c$/TestShare/folder1");
        try
        {
            using var file = File.Create($"//{Constants.LocalHost}/c$/TestShare/folder1/child1.txt");
            var result = _service.GetPathIntellisenseOptions($"//{Constants.LocalHost}/c$/TestShare/folder1/");
            result.Entries.Select(x => x.Name).Should().Contain("child1.txt");
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [WindowsOnlyFact]
    public void GetPathIntellisenseOptions_RootShare_RetrievesChildren()
    {
        var path = $"//{Constants.LocalHost}/c$/";

        var result = _service.GetPathIntellisenseOptions(path);

        result.Entries.Select(x => x.Name).Should().Contain("Program Files");
    }

    [WindowsOnlyFact]
    public void GetPathIntellisenseOptions_RootShareNoSeparator_ReturnsEmpty()
    {
        var path = $"//{Constants.LocalHost}/c$";

        var result = _service.GetPathIntellisenseOptions(path);

        result.Entries.Select(x => x.Name).Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public void GetPathIntellisenseOptions_Host_RetrievesAvailableShares()
    {
        var path = $"//{Constants.LocalHost}/";

        var result = _service.GetPathIntellisenseOptions(path);

        result
            .Entries.Select(x => x.Name)
            .Should()
            .Contain("C$");
    }

    [WindowsOnlyFact]
    public void GetPathIntellisenseOptions_HostNoSeparator_ReturnsEmpty()
    {
        var path = $"//{Constants.LocalHost}";

        var result = _service.GetPathIntellisenseOptions(path);

        result
            .Entries.Select(x => x.Name)
            .Should()
            .BeEmpty();
    }
}
