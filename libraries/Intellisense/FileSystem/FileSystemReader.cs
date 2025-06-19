using System.IO.Abstractions;
using NotNullStrings;

namespace Intellisense.FileSystem;

public interface IFileSystemReader
{
    IEnumerable<IFileSystemInfo> GetChildren(string path);
}

public class FileSystemReader(IFileSystem fileSystem) : IFileSystemReader
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public IEnumerable<IFileSystemInfo> GetChildren(string path)
    {
        if (path.IsBlank())
        {
            return [];
        }

        // will throw with null or whitespace
        var dir = fileSystem.DirectoryInfo.New(path);

        return !dir.Exists
            ? []
            : dir.EnumerateFileSystemInfos("*", EnumerationOptions);
    }
}
