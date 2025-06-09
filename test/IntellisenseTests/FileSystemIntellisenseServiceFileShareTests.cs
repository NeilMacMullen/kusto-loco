using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceFileShareTests
{
    private readonly IFileSystemIntellisenseService _service = new IntegrationTestContainer()
        .GetRequiredService<IFileSystemIntellisenseService>();

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_HostSep_ChildShares()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var sharePath = $"//{Constants.LocalHost}/";

        var result = await _service.GetPathIntellisenseOptionsAsync(sharePath);

        result
            .Entries
            .Should()
            .Contain(x => x.Name == "MyKustoTestShare1");
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_PartialShare_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        using var share2 = TestShare.Create("MyKustoTestShare2");
        var sharePath = $"//{Constants.LocalHost}/MyKust";

        var result = await _service.GetPathIntellisenseOptionsAsync(sharePath);

        result
            .Entries
            .Should()
            .Contain(x => x.Name == "MyKustoTestShare1")
            .And.Contain(x => x.Name == "MyKustoTestShare2");

        result.Filter.Should().Be("MyKust");
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_ShareNoSep_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        using var share2 = TestShare.Create("MyKustoTestShare2");
        var sharePath = $"//{Constants.LocalHost}/MyKustoTestShare1";

        var result = await _service.GetPathIntellisenseOptionsAsync(sharePath);

        result
            .Entries
            .Should()
            .Contain(x => x.Name == "MyKustoTestShare1")
            .And.Contain(x => x.Name == "MyKustoTestShare2");

        result.Filter.Should().Be("MyKustoTestShare1");
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_ShareSep_ChildFiles()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var sharePath = $"//{Constants.LocalHost}/MyKustoTestShare1/";


        var files = new[] { "myKustoFile1.txt", "myKustoFile2.txt" };
        share.Folder.TouchFiles(files);

        var result = await _service.GetPathIntellisenseOptionsAsync(sharePath);

        result.Entries.Select(x => x.Name).Should().BeEquivalentTo(files);
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_PartialShareChildFolder_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var path = $"//{Constants.LocalHost}/MyKustoTestShare1/MyKustoF";

        share.Folder.CreateSubdirectory("MyKustoFolder1");
        share.Folder.CreateSubdirectory("MyKustoFolder2");

        var result = await _service.GetPathIntellisenseOptionsAsync(path);

        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyKustoFolder1", "MyKustoFolder2");
        result.Filter.Should().Be("MyKustoF");
    }

    [WindowsAdminCiFact]
    public async Task GetPathIntellisenseOptions_ShareChildFolderNoSep_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var path = $"//{Constants.LocalHost}/MyKustoTestShare1/MyKustoFolder1";
        share.Folder.CreateSubdirectory("MyKustoFolder1");
        share.Folder.CreateSubdirectory("MyKustoFolder2");
        var result = await _service.GetPathIntellisenseOptionsAsync(path);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyKustoFolder1", "MyKustoFolder2");
        result.Filter.Should().Be("MyKustoFolder1");
    }
}
