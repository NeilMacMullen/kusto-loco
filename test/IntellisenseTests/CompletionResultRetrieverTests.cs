using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace IntellisenseTests;

public class CompletionResultRetrieverTests
{
    private readonly IPathFactory _pathFactory = new ServiceCollection()
        .AddMockedIo()
        .BuildServiceProvider()
        .GetRequiredService<IPathFactory>();

    [WindowsOnlyTheory]
    [InlineData("C:/", new[] { "Folder1", "Folder2" })]
    [InlineData("D:/", new[] { "File1.txt" })]
    [InlineData("/", new[] { "Folder1", "Folder2" })]
    [InlineData("\\", new[] { "Folder1", "Folder2" })]
    public void Children_GetCompletions_Roots_RetrievesChildrenOfRoot(string path, string[] expected)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/Folder1/Folder11"] = new(""),
                ["C:/Folder1/File11.txt"] = new(""),
                ["C:/Folder2/File22.txt"] = new(""),
                ["D:/File1.txt"] = new("")
            },
            new MockFileSystemOptions { CreateDefaultTempDir = false }
        );
        var reader = new FileSystemReader(fileSystem);
        var retriever = new ChildrenPathCompletionResultRetriever(reader, Mock.Of<IShareReader>());

        var rootedPath = _pathFactory.CreateOrThrow<WindowsRootedPath>(path);
        var result = retriever.GetCompletionResult(rootedPath);

        result
            .Entries.Select(x => x.Name)
            .Should()
            .BeEquivalentTo(expected);
    }

    [WindowsOnlyTheory]
    [InlineData("C:./")]
    [InlineData("C:.")]
    [InlineData("C:")]
    public void Children_GetCompletions_RelativeToDriveRoot_ReturnsEmpty(string path)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/Folder1/Folder11"] = new(""),
                ["C:/Folder1/File11.txt"] = new(""),
                ["C:/Folder2/File22.txt"] = new("")
            },
            new MockFileSystemOptions { CreateDefaultTempDir = false }
        );
        var reader = new FileSystemReader(fileSystem);

        var retriever = new ChildrenPathCompletionResultRetriever(reader, Mock.Of<IShareReader>());
        var rootedPath = _pathFactory.CreateOrThrow<WindowsRootedPath>(path);

        retriever
            .GetCompletionResult(rootedPath)
            .Entries.Should()
            .BeEmpty();
    }

    [WindowsOnlyTheory]
    [InlineData("/Folder1/")]
    [InlineData("C:/Folder1/")]
    public void Children_GetCompletions_ChildDirectoryEndingInSep_RetrievesChildren(string path)
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/Folder1/Folder11"] = new(""),
                ["C:/Folder1/File11.txt"] = new(""),
                ["C:/Folder2/File22.txt"] = new("")
            }
        );
        var retriever = new ChildrenPathCompletionResultRetriever(new FileSystemReader(fileSystem),Mock.Of<IShareReader>());


        var rootedPath = _pathFactory.CreateOrThrow<WindowsRootedPath>(path);
        var result = retriever.GetCompletionResult(rootedPath);

        using var _ = new AssertionScope();

        result.Should().NotBeNull();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder11", "File11.txt");
        result.Filter.Should().BeEmpty();
    }

    [WindowsOnlyFact]
    public void Sibling_GetCompletions_PartialPath_RetrievesSiblings()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/Folder1/Folder11"] = new(""),
                ["C:/Folder1/File11.txt"] = new(""),
                ["C:/Folder2/File22.txt"] = new("")
            },
            new MockFileSystemOptions { CreateDefaultTempDir = false }
        );
        var retriever = new SiblingPathCompletionResultRetriever(new FileSystemReader(fileSystem));

        var rootedPath = _pathFactory.CreateOrThrow<WindowsRootedPath>("C:/Folder1");
        var result = retriever.GetCompletionResult(rootedPath);

        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
        result.Filter.Should().Be("Folder1");
    }
}
