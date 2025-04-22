namespace Intellisense.FileSystem.Paths;

/// <summary>
/// Guarantees that the path either starts with a directory separator (i.e. /) or a drive letter followed by a volume separator (i.e. C:).
/// </summary>
internal class WindowsRootedPath : IFileSystemPath
{
    private readonly string _value;

    internal WindowsRootedPath(string path)
    {
        _value = path;
    }

    public string GetPath() => _value;

    public bool IsRootDirectory()
    {
        if (_value[0].IsDirectorySeparator())
        {
            // TODO: Handle UNC paths
            return _value.Length is 1;
        }

        if (_value.Length is 2)
        {
            return false;
        }

        return _value.Length is 3 && _value[2].IsDirectorySeparator();
    }
}