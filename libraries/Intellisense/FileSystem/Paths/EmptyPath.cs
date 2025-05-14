namespace Intellisense.FileSystem.Paths;


internal class EmptyPath : IFileSystemPath
{
    public string GetPath() => string.Empty;

    public bool IsRootDirectory() => false;

    public static readonly EmptyPath Instance = new();
}
