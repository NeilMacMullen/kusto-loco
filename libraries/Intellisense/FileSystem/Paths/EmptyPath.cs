namespace Intellisense.FileSystem.Paths;


internal class EmptyPath(string value) : FileSystemPath(value)
{
    public static readonly EmptyPath Instance = new("");
    public override bool IsRootDirectory => false;
}
