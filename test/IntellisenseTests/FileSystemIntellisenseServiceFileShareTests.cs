using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Intellisense;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Xunit;


namespace IntellisenseTests;

[CollectionDefinition(nameof(FileShareTestCollection), DisableParallelization = true)]
public class FileShareTestCollection;

[Collection(nameof(FileShareTestCollection))]
public class FileSystemIntellisenseServiceFileShareTests : IClassFixture<FileShareTestCollection>
{
    private readonly IFileSystemIntellisenseService _service;
    // ReSharper disable once NotAccessedField.Local
    private readonly FileShareTestCollection _collection;


    public FileSystemIntellisenseServiceFileShareTests(FileShareTestCollection collection)
    {
        _service = new ServiceCollection()
            .AddDefault()
            .BuildServiceProvider()
            .GetRequiredService<IFileSystemIntellisenseService>();

        _collection = collection;
    }

    [WindowsAdminOnlyFact] // will change => siblings in later feature
    public void GetPathIntellisenseOptions_PartialHost_Empty()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var sharePath = "//localhos";

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result.Entries.Should().BeEmpty();
    }

    [WindowsAdminOnlyFact] // will change => siblings in later feature
    public void GetPathIntellisenseOptions_HostNoSep_Empty()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var sharePath = $"//{Constants.LocalHost}";

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result.Entries.Should().BeEmpty();
    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_HostSep_ChildShares()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var sharePath = $"//{Constants.LocalHost}/";

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result
            .Entries
            .Should()
            .Contain(x => x.Name == "MyKustoTestShare1");
    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_PartialShare_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        using var share2 = TestShare.Create("MyKustoTestShare2");
        var sharePath = $"//{Constants.LocalHost}/MyKust";

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result
            .Entries
            .Should()
            .Contain(x => x.Name == "MyKustoTestShare1")
            .And.Contain(x => x.Name == "MyKustoTestShare2");
    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_ShareNoSep_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        using var share2 = TestShare.Create("MyKustoTestShare2");
        var sharePath = $"//{Constants.LocalHost}/MyKustoTestShare1";

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result
            .Entries
            .Should()
            .Contain(x => x.Name == "MyKustoTestShare1")
            .And.Contain(x => x.Name == "MyKustoTestShare2");
    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_ShareSep_ChildFiles()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var sharePath = $"//{Constants.LocalHost}/MyKustoTestShare1/";


        var files = new []{ "myKustoFile1.txt","myKustoFile2.txt" };
        share.Folder.TouchFiles(files);

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result.Entries.Select(x => x.Name).Should().BeEquivalentTo(files);
    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_PartialShareChildFolder_Siblings()
    {

        using var share = TestShare.Create("MyKustoTestShare1");
        var path = $"//{Constants.LocalHost}/MyKustoTestShare1/MyKustoF";

        share.Folder.CreateSubdirectory("MyKustoFolder1");
        share.Folder.CreateSubdirectory("MyKustoFolder2");

        var result = _service.GetPathIntellisenseOptions(path);

        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyKustoFolder1", "MyKustoFolder2");


    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_ShareChildFolderNoSep_Siblings()
    {
        using var share = TestShare.Create("MyKustoTestShare1");
        var path = $"//{Constants.LocalHost}/MyKustoTestShare1/MyKustoFolder1";
        share.Folder.CreateSubdirectory("MyKustoFolder1");
        share.Folder.CreateSubdirectory("MyKustoFolder2");
        var result = _service.GetPathIntellisenseOptions(path);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyKustoFolder1", "MyKustoFolder2");
    }
}

internal static class FileSystemExtensions
{
    public static void Touch(this FileInfo file)
    {
        if (file.Directory is not { } dir)
        {
            throw new NotImplementedException();
        }

        if (!dir.Exists)
        {
            dir.Create();
        }

        file.Create().Dispose();
    }

    public static List<FileInfo> TouchFiles(this DirectoryInfo directory, IEnumerable<string> filePaths) =>
        filePaths.Select(directory.TouchFile).ToList();

    public static FileInfo TouchFile(this DirectoryInfo directory, string fileName)
    {
        var path = Path.Combine(directory.FullName, fileName);
        var file = new FileInfo(path);
        file.Touch();
        return file;
    }
}
