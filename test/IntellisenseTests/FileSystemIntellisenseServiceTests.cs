using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    private readonly FileSystemIntellisenseService _fileSystemIntellisenseService;

    public FileSystemIntellisenseServiceTests()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/File1.txt"] = new(""),
                ["C:/File2.txt"] = new(""),
                ["C:/Folder1/File1.txt"] = new(""),
                ["C:/Folder1/File2.txt"] = new(""),
                ["C:/Folder1/MyFile1.txt"] = new(""),
                ["C:/Folder1/MyFile2.txt"] = new(""),
                ["C:/Folder1/Folder2"] = new MockDirectoryData(),
                ["C:/Folder2/File1.txt"] = new("")
            },
            new MockFileSystemOptions
            {
                CreateDefaultTempDir = false
            }
        );
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(fileSystem);
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentDir/").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirNoDirectorySeparatorSuffix_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentDir").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparator_SiblingPathsMatchingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1");

        result.Rewind.Should().Be("Folder1".Length);
        result.Prefix.Should().Be(string.Empty);

        result
            .Entries.Select(x => x.Name)
            .Should()
            .BeEquivalentTo("Folder1","Folder2","File1.txt","File2.txt");
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/");

        result.Prefix.Should().Be("");
        result.Rewind.Should().Be(0);

        result
            .Entries
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo(
                "File1.txt",
                "File2.txt",
                "MyFile1.txt",
                "MyFile2.txt",
                "Folder2"
            );
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_DirectoryChildren()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("C:/");

        result.Prefix.Should().Be("");
        result.Rewind.Should().Be(0);

        result
            .Entries
            .Select(x => x.Name)
            .Should()
            .BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirNoDirectorySeparatorSuffixAtRoot_Empty()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("C:");
        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_ExclusiveValidFilePath_SingleFile()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/MyFile1.txt");

        result.Rewind.Should().Be("MyFile1.txt".Length);
        result.Prefix.Should().Be(string.Empty);
        result.Entries.Should().ContainSingle().Which.Name.Should().Be("MyFile1.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInput_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/Folder1/MyF");

        result.Rewind.Should().Be(3);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialDirInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("C:/Fol");

        result.Rewind.Should().Be("Fol".Length);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFileInputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("C:/Fil");

        result.Rewind.Should().Be("Fil".Length);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt");
    }

    [Fact]
    public void GetPathIntellisenseOptions_InputAtRoot_PathsContainingInput()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("C:/F");

        result.Rewind.Should().Be("F".Length);
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentPath_Empty()
    {
        var results =
            _fileSystemIntellisenseService.GetPathIntellisenseOptions("/NonExistentPath.txt").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("D:").Entries;

        results.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonRootedPath_Empty()
    {
        var results = _fileSystemIntellisenseService.GetPathIntellisenseOptions("./Folder1/MyFile1.txt").Entries;

        results.Should().BeEmpty();
    }
}

public class FileSystemIntellisenseServiceIntegrationTests
{
    private readonly FileSystemIntellisenseService _fileSystemIntellisenseService;

    public FileSystemIntellisenseServiceIntegrationTests()
    {
        _fileSystemIntellisenseService = new FileSystemIntellisenseService(new FileSystem());
    }

    [Fact]
    public void
        GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffixAtRoot_HasChild()
    {
        var result = _fileSystemIntellisenseService.GetPathIntellisenseOptions("/");

        result.Prefix.Should().Be("");
        result.Rewind.Should().Be(0);

        result
            .Entries
            .Take(1)
            .Should()
            .NotBeEmpty();
    }
}
