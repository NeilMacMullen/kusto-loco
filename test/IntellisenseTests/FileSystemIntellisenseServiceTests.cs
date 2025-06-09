using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using AwesomeAssertions.Execution;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Xunit;


namespace IntellisenseTests;

public class FileSystemIntellisenseServiceTests
{
    [Fact]
    public async Task GetPathIntellisenseOptions_ExistentRootNonExistentChildDir_RetrievesSiblings()
    {
        var data = new List<string> { "/Folder1" };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        var result = await f.GetPathIntellisenseOptionsAsync("/NonExistentDir");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1");
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_ValidDirNoDirectorySeparator_RetrievesSiblings()
    {
        var data = new List<string>
        {
            "/File1.txt",
            "/Folder1/File1.txt",
            "/Folder1abc/File1.txt",
            "/Folder2/File1.txt"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1", "Folder1abc", "Folder2");
        result.Filter.Should().Be("Folder1");
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_RootWithDirSeparator_RetrievesChildren()
    {
        var data = new List<string>
        {
            "/File1.txt",
            "/File2.txt",
            "/Folder1/File1.txt",
            "/Folder2/File1.txt"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "File2.txt", "Folder1", "Folder2");
        result.Filter.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_PartialPathAtRoot_RetrievesRootChildren()
    {
        var data = new List<string>
        {
            "/File1.txt",
            "/Folder1"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/F");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1");
        result.Filter.Should().Be("F");
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_NonexistentDirDirectorySeparatorSuffix_ReturnsEmptyResult()
    {
        var data = new List<string> { "/Folder1" };

        var f = new FileSystemIntellisenseServiceTestFixture(data);

        var result = await f.GetPathIntellisenseOptionsAsync("/NonExistentDir/");

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_ValidDirDirectorySeparatorSuffix_RetrievesChildren()
    {
        var data = new List<string>
        {
            "/Folder1/File1.txt",
            "/Folder2/File2.txt"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1/");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt");
        result.Filter.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_PartialDir_RetrievesSiblings()
    {
        var data = new List<string>
        {
            "/Folder1",
            "/Folder2"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Fol");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
        result.Filter.Should().Be("Fol");
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_PartialFile_RetrievesSiblings()
    {
        var data = new List<string>
        {
            "/Folder1/MyFile1.txt",
            "/Folder1/MyFile2.txt"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1/MyF");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("MyFile1.txt", "MyFile2.txt");
        result.Filter.Should().Be("MyF");
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_PartialPath_RetrievesSiblings()
    {
        var data = new List<string>
        {
            "/File1.txt",
            "/Folder1"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Fil");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File1.txt", "Folder1");
        result.Filter.Should().Be("Fil");
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_NonexistentParent_ReturnsEmptyResult()
    {
        var data = new List<string> { "/Folder1" };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Abc/Folder1");

        result.Entries.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_NonexistentRootPath_ReturnsEmptyResult()
    {
        var data = new List<string> { "C:/Folder1" };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("D:/");

        result.Entries.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public async Task GetPathIntellisenseOptions_NonexistentDirAndChild_ReturnsEmptyResult()
    {
        var data = new List<string> { "/Folder1" };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Abc/def.txt");

        result.Entries.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPathIntellisenseOptions_RootedRelativePath_RelativeChildren()
    {
        var data = new List<string>
        {
            "/File1.txt",
            "/Folder2/File1.txt",
            "/Folder1/Folder2/Folder3/Folder4",
            "/Folder1/Folder2/File5.txt"
        };

        var f = new FileSystemIntellisenseServiceTestFixture(data);
        var result = await f.GetPathIntellisenseOptionsAsync("/Folder1/Folder2/Folder3/Folder4/../../");

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("File5.txt", "Folder3");
        result.Filter.Should().BeEmpty();
    }
}