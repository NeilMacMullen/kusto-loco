namespace Intellisense.FileSystem.Paths;

internal interface IFileSystemPath
{
    string GetPath();
    bool IsRootDirectory();
}

