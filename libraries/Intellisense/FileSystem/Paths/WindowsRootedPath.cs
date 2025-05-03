namespace Intellisense.FileSystem.Paths;

/// <summary>
/// Guarantees that the path either starts with a directory separator (i.e. /) or a drive letter followed by a volume separator (i.e. C:).
/// </summary>
internal class WindowsRootedPath(string path) : FileSystemPath(path)
{
    public override bool IsRootDirectory
    {
        get
        {
            if (Value[0].IsDirectorySeparator())
            {
                return Value.Length is 1;
            }

            if (Value.Length is 2)
            {
                return false;
            }

            return Value.Length is 3 && Value[2].IsDirectorySeparator();
        }
    }
}
