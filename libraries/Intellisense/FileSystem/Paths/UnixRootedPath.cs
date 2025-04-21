namespace Intellisense.FileSystem.Paths;

internal class UnixRootedPath(string path) : IFileSystemPath
{
    public string GetPath() => path;

    public bool IsRootDirectory() => path.Length is 1 && path[0] == Path.DirectorySeparatorChar;
}
