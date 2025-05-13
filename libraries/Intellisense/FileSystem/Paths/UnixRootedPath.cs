namespace Intellisense.FileSystem.Paths;

internal class UnixRootedPath(string path) : FileSystemPath(path)
{
    public override bool IsRootDirectory => Value.Length is 1 && Value[0] == Path.DirectorySeparatorChar;
}
