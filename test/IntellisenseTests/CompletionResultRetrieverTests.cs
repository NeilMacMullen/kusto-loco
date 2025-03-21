﻿using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using Xunit;

namespace IntellisenseTests;

public class CompletionResultRetrieverTests
{
    [Theory]
    [InlineData("C:/",new[]{"Folder1","Folder2"})]
    [InlineData("D:/",new[]{"File1.txt"})]
    [InlineData("/",new[]{"Folder1","Folder2"})]
    [InlineData("\\",new[]{"Folder1","Folder2"})]
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
        var retriever = new ChildrenPathCompletionResultRetriever(reader);

        var rootedPath = RootedPath.CreateOrThrow(path);
        var result = retriever.GetCompletionResult(rootedPath);

        result
            .Entries.Select(x => x.Name)
            .Should()
            .BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData("//uncHost/c/", new[] { "folderC1", "folderC2" })]
    [InlineData("//uncHost/d/", new[] { "folderD1" })]
    public void Children_GetCompletions_UncRootPaths_RetrievesChildrenOfRootRelativeToHost(
        string path,
        string[] expected
    )
    {
        var data = new Dictionary<string, MockFileData>
        {
            ["//uncHost/c/folderC1/fileC11.txt"] = new(""),
            ["//uncHost/c/folderC2/fileC21.txt"] = new(""),
            ["//uncHost/d/folderD1/fileD11.txt"] = new("")
        };
        var fileSystem = new MockFileSystem(data, new MockFileSystemOptions { CreateDefaultTempDir = false });
        var reader = new FileSystemReader(fileSystem);
        var retriever = new ChildrenPathCompletionResultRetriever(reader);

        var rootedPath = RootedPath.CreateOrThrow(path);
        var result = retriever.GetCompletionResult(rootedPath);

        result.Entries.Select(x => x.Name).Should().BeEquivalentTo(expected);
    }

    [Theory]
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

        var retriever = new ChildrenPathCompletionResultRetriever(reader);
        var rootedPath = RootedPath.CreateOrThrow(path);

        retriever
            .GetCompletionResult(rootedPath)
            .Entries.Should()
            .BeEmpty();
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
        var retriever = new ChildrenPathCompletionResultRetriever(new FileSystemReader(fileSystem));


        var rootedPath = RootedPath.CreateOrThrow(path);
        var result = retriever.GetCompletionResult(rootedPath);

        using var _ = new AssertionScope();

        result.Should().NotBeNull();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder11", "File11.txt");
        result.Filter.Should().BeEmpty();
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
            new MockFileSystemOptions { CreateDefaultTempDir = false }
        );
        var retriever = new SiblingPathCompletionResultRetriever(new FileSystemReader(fileSystem));

        var rootedPath = RootedPath.CreateOrThrow("C:/Folder1");
        var result = retriever.GetCompletionResult(rootedPath);

        using var _ = new AssertionScope();
        result.Should().NotBeNull();
        result.Entries.Select(x => x.Name).Should().BeEquivalentTo("Folder1", "Folder2");
        result.Filter.Should().Be("Folder1");
    }
}
