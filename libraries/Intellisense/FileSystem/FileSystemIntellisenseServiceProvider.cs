using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging;

namespace Intellisense.FileSystem;

public static class FileSystemIntellisenseServiceProvider
{
    private static readonly System.IO.Abstractions.FileSystem FileSystem = new();
    public static IFileSystemIntellisenseService GetFileSystemIntellisenseService()
    {
        var logger = new LoggerFactory().CreateLogger<FileSystemIntellisenseService>();
        var fileSystemReader = new FileSystemReader(FileSystem);
        return new FileSystemIntellisenseService(fileSystemReader,logger,new RootedPathFactory());
    }
}
