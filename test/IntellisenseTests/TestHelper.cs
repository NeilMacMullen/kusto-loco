using System;
using System.IO;
using System.IO.Abstractions;
using Intellisense.FileSystem;

namespace IntellisenseTests;

public static  class TestHelper
{
    public static FileSystem FileSystem { get; } = new();
    public static Guid RunId { get; } = Guid.NewGuid();

    public static IDirectoryInfo TestDirectory { get; } =
        FileSystem.DirectoryInfo.New(Path.Combine(Path.GetTempPath(), nameof(IntellisenseTests)));

    public static IDirectoryInfo RunDirectory { get; } = FileSystem.DirectoryInfo.New(Path.Combine(TestDirectory.FullName, RunId.ToString()));
    public static IDirectoryInfo MethodDirectory() => FileSystem.DirectoryInfo.New(Path.Combine(RunDirectory.FullName, $"{Guid.NewGuid()}"));

    public static IDirectoryInfo CreateCleanTestDirectory()
    {
        var dir = MethodDirectory();
        dir.CreateOrClean();
        return dir;
    }

}
