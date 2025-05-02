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

namespace IntellisenseTests;

public sealed class FileSystemIntellisenseServiceNetworkShareTests
{
    private readonly IFileSystemIntellisenseService _service;

    public FileSystemIntellisenseServiceNetworkShareTests()
    {
        _service = new ServiceCollection()
            .AddDefault()
            .BuildServiceProvider()
            .GetRequiredService<IFileSystemIntellisenseService>();
    }

    [WindowsAdminOnlyFact]
    public void GetPathIntellisenseOptions_MappedLocalDrive_RetrievesChildren()
    {
        using var share = new TestShare();
        var testFiles = new[] { "testfile1.txt", "testfile2.txt" };
        var sharePath = $"//{Constants.LocalHost}/{share.Name}/";



        share.Folder.TouchFiles(testFiles);

        var result = _service.GetPathIntellisenseOptions(sharePath);

        result
            .Entries
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo(testFiles);
    }
}


file static class FileSystemExtensions
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

    public static List<FileInfo> TouchFiles(this DirectoryInfo directory, IEnumerable<string> filePaths) => filePaths.Select(directory.TouchFile).ToList();

    public static FileInfo TouchFile(this DirectoryInfo directory, string fileName)
    {
        var path = Path.Combine(directory.FullName, fileName);
        var file = new FileInfo(path);
        file.Touch();
        return file;
    }
}
