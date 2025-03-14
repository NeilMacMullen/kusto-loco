using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public static class FileSystemIntellisenseServiceProvider
{
    private static readonly System.IO.Abstractions.FileSystem FileSystem = new();
    public static IFileSystemIntellisenseService GetFileSystemIntellisenseService()
    {
        return GetFileSystemIntellisenseService(FileSystem);
    }

    public static IFileSystemIntellisenseService GetFileSystemIntellisenseService(IFileSystem fileSystem)
    {
        var fileSystemReader = new FileSystemReader(fileSystem);
        return new FileSystemIntellisenseService(fileSystemReader);
    }
}
