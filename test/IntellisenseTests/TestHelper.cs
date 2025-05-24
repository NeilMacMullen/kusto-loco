using System;
using System.IO;
using System.IO.Abstractions;

namespace IntellisenseTests;

public static class TestHelper
{
    private static FileSystem FileSystem { get; } = new();
    private static Guid RunId { get; } = Guid.NewGuid();

    public static IDirectoryInfo TestDirectory { get; } = FileSystem.DirectoryInfo.New(Path.Combine(Path.GetTempPath(), nameof(IntellisenseTests)));

    private static IDirectoryInfo RunDirectory { get; } = FileSystem.DirectoryInfo.New(Path.Combine(TestDirectory.FullName, RunId.ToString()));
    private static IDirectoryInfo IndividualTestDirectory(string name) => FileSystem.DirectoryInfo.New(Path.Combine(RunDirectory.FullName, name));

    public static IDirectoryInfo CreateCleanTestDirectory(string name = "")
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = Guid.NewGuid().ToString();
        }

        var dir = IndividualTestDirectory(name);
        dir.CreateOrClean();
        return dir;
    }

}
