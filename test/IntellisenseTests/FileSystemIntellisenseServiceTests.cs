using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Intellisense;
using Intellisense.FileSystem;
using Xunit;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    [Fact]
    public void GetPathIntellisenseOptions_ExistentRootNonExistentChildDir_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        var result = f.GetPathIntellisenseOptions("/NonExistentDir");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1");
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirNoDirectorySeparator_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/Folder1/File1.txt"] = new(""),
            ["C:/Folder1abc/File1.txt"] = new(""),
            ["C:/Folder2/File1.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("/Folder1");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1", "Folder1abc", "Folder2");
        result.Filter.Should().Be("Folder1");
    }

    [Fact]
    public void GetPathIntellisenseOptions_RootWithDirSeparator_RetrievesChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/File2.txt"] = new(""),
            ["C:/Folder1/File1.txt"] = new(""),
            ["C:/Folder2/File1.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("C:/");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
        result.Filter.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialPathAtRoot_RetrievesRootChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("/F");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1");
        result.Filter.Should().Be("F");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        var result = f.GetPathIntellisenseOptions("/NonExistentDir/");

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_RetrievesChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1/File1.txt"] = new(""),
            ["C:/Folder2/File2.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("/Folder1/");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt");
        result.Filter.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialDir_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new(""),
            ["C:/Folder2"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("C:/Fol");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
        result.Filter.Should().Be("Fol");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialFile_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1/MyFile1.txt"] = new(""),
            ["C:/Folder1/MyFile2.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("C:/Folder1/MyF");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
        result.Filter.Should().Be("MyF");
    }

    [Fact]
    public void GetPathIntellisenseOptions_PartialPath_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("C:/Fil");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1");
        result.Filter.Should().Be("Fil");
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentParent_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("/Abc/Folder1");

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentRootPath_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("D:/");

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_NonexistentDirAndChild_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("C:/Abc/def.txt");

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public void GetPathIntellisenseOptions_RootedRelativePath_RelativeChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new MockDirectoryData(),
            ["C:/Folder2/File1.txt"] = new MockDirectoryData(),
            ["C:/Folder1/Folder2/Folder3/Folder4"] = new MockDirectoryData(),
            ["C:/Folder1/Folder2/File5.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = f.GetPathIntellisenseOptions("/Folder1/Folder2/Folder3/Folder4/../../");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File5.txt", "Folder3");
        result.Filter.Should().BeEmpty();
    }
}

file class FileSystemIntellisenseServiceTestFixture
{
    public IFileSystem FileSystem { get; }
    public IFileSystemIntellisenseService FileSystemIntellisenseService { get; }

    public FileSystemIntellisenseServiceTestFixture(Dictionary<string, MockFileData> fileData, MockFileSystemOptions? options = null)
    {
        options ??= new MockFileSystemOptions { CreateDefaultTempDir = false };
        FileSystem = new MockFileSystem(fileData, options);
        FileSystemIntellisenseService =
            FileSystemIntellisenseServiceProvider.GetFileSystemIntellisenseService(FileSystem);
    }

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        return FileSystemIntellisenseService.GetPathIntellisenseOptions(path);
    }
}
