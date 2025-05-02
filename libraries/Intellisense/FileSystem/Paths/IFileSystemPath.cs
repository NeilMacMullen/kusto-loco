namespace Intellisense.FileSystem.Paths;

internal interface IFileSystemPath
{
    string GetPath();
    bool IsRootDirectory();
    string GetParent() => Path.GetDirectoryName(GetPath()) ?? string.Empty;
}

