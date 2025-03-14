using System.IO.Abstractions;

namespace Intellisense.FileSystem;

internal interface IFileSystemReader
{
    IEnumerable<IFileSystemInfo> GetChildren(string path);
}

internal class FileSystemReader(IFileSystem fileSystem) : IFileSystemReader
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public IEnumerable<IFileSystemInfo> GetChildren(string path)
    {
        var dir = fileSystem.DirectoryInfo.New(path);

        if (!dir.Exists)
        {
            return [];
        }

        return dir.EnumerateFileSystemInfos("*", EnumerationOptions);
    }
}
