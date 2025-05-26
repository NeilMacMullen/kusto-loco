using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Xunit;


namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_ExistentRootNonExistentChildDir_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        var result = await f.GetPathIntellisenseOptionsAsync("/NonExistentDir");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1");
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_ValidDirNoDirectorySeparator_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/Folder1/File1.txt"] = new(""),
            ["C:/Folder1abc/File1.txt"] = new(""),
            ["C:/Folder2/File1.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1", "Folder1abc", "Folder2");
        result.Filter.Should().Be("Folder1");
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_RootWithDirSeparator_RetrievesChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/File2.txt"] = new(""),
            ["C:/Folder1/File1.txt"] = new(""),
            ["C:/Folder2/File1.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("C:/");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
        result.Filter.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_PartialPathAtRoot_RetrievesRootChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/F");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1");
        result.Filter.Should().Be("F");
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        var result = await f.GetPathIntellisenseOptionsAsync("/NonExistentDir/");

        result.Entries.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_RetrievesChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1/File1.txt"] = new(""),
            ["C:/Folder2/File2.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1/");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt");
        result.Filter.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_PartialDir_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new(""),
            ["C:/Folder2"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("C:/Fol");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
        result.Filter.Should().Be("Fol");
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_PartialFile_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1/MyFile1.txt"] = new(""),
            ["C:/Folder1/MyFile2.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("C:/Folder1/MyF");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
        result.Filter.Should().Be("MyF");
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_PartialPath_RetrievesSiblings()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new(""),
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("C:/Fil");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1");
        result.Filter.Should().Be("Fil");
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_NonexistentParent_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Abc/Folder1");

        result.Entries.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_NonexistentRootPath_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("D:/");

        result.Entries.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_NonexistentDirAndChild_ReturnsEmptyResult()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("C:/Abc/def.txt");

        result.Entries.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_RootedRelativePath_RelativeChildren()
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/File1.txt"] = new MockDirectoryData(),
            ["C:/Folder2/File1.txt"] = new MockDirectoryData(),
            ["C:/Folder1/Folder2/Folder3/Folder4"] = new MockDirectoryData(),
            ["C:/Folder1/Folder2/File5.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1/Folder2/Folder3/Folder4/../../");

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
    public async Task GetPathIntellisenseOptions_UnusualPaths_ShouldNotThrow(string path)
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["C:/Folder1/File1.txt"] = new("")
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        await f
            .Invoking(async x => await x.GetPathIntellisenseOptionsAsync(path))
            .Should()
            .NotThrowAsync();
    }
}
