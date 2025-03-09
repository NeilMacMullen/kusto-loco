namespace Intellisense.FileSystem;

public static class FileSystemIntellisenseServiceProvider
{
    private static readonly System.IO.Abstractions.FileSystem FileSystem = new();
    public static IFileSystemIntellisenseService GetFileSystemIntellisenseService()
    {
        return new FileSystemIntellisenseService(FileSystem);
    }
}
