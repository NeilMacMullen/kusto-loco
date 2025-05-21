using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Moq;
using Xunit;


namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyFact]
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

    [WindowsOnlyTheory]
    [InlineData("//")]
    [InlineData(@"\\")]
    [InlineData("/:")]
    [InlineData("/.")]
    [InlineData("/a/b")]
    [InlineData("C://")]
    [InlineData("C:/a")]
    public void GetPathIntellisenseOptions_UnusualPaths_ShouldNotThrow(string path)
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1/File1.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        f
            .Invoking(x => x.GetPathIntellisenseOptions(path))
            .Should()
            .NotThrow();
    }

    [WindowsOnlyFact]
    public void GetPathIntellisenseOptions_IOException_LogsError()
    {

        var mock = new Mock<IFileSystemReader>();
        mock.Setup(x => x.GetChildren(It.IsAny<string>())).Throws<PathTooLongException>();

        var f = new FileSystemIntellisenseServiceTestFixture(mock.Object);

        f.GetLogs().Should().NotContain(x => x.Exception is IOException);

        f
            .Invoking(x => x.GetPathIntellisenseOptions(""))
            .Should()
            .NotThrow();

        f.GetLogs().Should().ContainSingle(x => x.Exception is IOException);
    }
}