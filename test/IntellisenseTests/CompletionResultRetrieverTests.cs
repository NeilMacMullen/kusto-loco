using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Xunit;

namespace IntellisenseTests;

public class CompletionResultRetrieverTests
{

    [Theory]
    [InlineData("C:/")]
    [InlineData("D:/")]
    [InlineData("/")]
    [InlineData("\\")]
    public void Root_CanHandle_Roots_ReturnsTrue(string path)
    {
        var retriever = new RootChildrenRootedPathCompletionResultRetriever(new MockReader());

        retriever.CanHandle(RootedPath.CreateOrThrow(path)).Should().BeTrue();
    }

    [Theory]
    [InlineData("C:./")]
    [InlineData("C:.")]
    public void Root_CanHandle_NotRoot_ReturnsFalse(string path)
    {
        var retriever = new RootChildrenRootedPathCompletionResultRetriever(new MockReader());

        retriever.CanHandle(RootedPath.CreateOrThrow(path)).Should().BeFalse();
    }

    [Theory]
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
        var retriever = new ChildrenRootedPathCompletionResultRetriever(new FileSystemReader(fileSystem));

        var result = retriever.GetCompletionResult(RootedPath.CreateOrThrow(path));

        using var _ = new AssertionScope();

        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder11", "File11.txt");
        result.Filter.Should().BeEmpty();
    }

    [Theory]
    [InlineData("/Folder1/")]
    [InlineData("C:/Folder1/")]
    public void Children_CanHandle_ChildDirectoryEndingInSep_ReturnsTrue(string path)
    {
        var retriever = new ChildrenRootedPathCompletionResultRetriever(new MockReader());

        var result = retriever.CanHandle(RootedPath.CreateOrThrow(path));

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("/Folder1")]
    [InlineData("C:/Folder1")]
    public void Sibling_CanHandle_PartialPath_ReturnsTrue( string path)
    {

        var retriever = new SiblingRootedPathCompletionResultRetriever(new MockReader());

        var result = retriever.CanHandle(RootedPath.CreateOrThrow(path));

        result.Should().BeTrue();
    }

    [Fact]
    public void Sibling_GetCompletions_PartialPath_RetrievesSiblings()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                ["C:/Folder1/Folder11"] = new(""),
                ["C:/Folder1/File11.txt"] = new(""),
                ["C:/Folder2/File22.txt"] = new("")
            },
            new MockFileSystemOptions{CreateDefaultTempDir = false}
        );
        var retriever = new SiblingRootedPathCompletionResultRetriever(new FileSystemReader(fileSystem));

        var result = retriever.GetCompletionResult(RootedPath.CreateOrThrow("C:/Folder1"));

        using var _ = new AssertionScope();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1","Folder2");
        result.Filter.Should().Be("Folder1");
    }

}

file class MockReader : IFileSystemReader
{

    public IEnumerable<IFileSystemInfo> GetChildren(string path)
    {
        throw new NotImplementedException();
    }
}
