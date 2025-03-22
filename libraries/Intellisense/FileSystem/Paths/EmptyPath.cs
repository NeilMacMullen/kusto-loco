namespace Intellisense.FileSystem.Paths;


internal class EmptyPath : IFileSystemPath
{
    public string GetPath()
    {
        return string.Empty;
    }

    public bool IsRootDirectory()
    {
        return false;
    }

    public static readonly EmptyPath Instance = new();
}
