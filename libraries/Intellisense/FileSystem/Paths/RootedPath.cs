namespace Intellisense.FileSystem.Paths;

/// <summary>
/// Guarantees that the path either starts with a directory separator (i.e. /) or a drive letter followed by a volume separator (i.e. C:).
/// </summary>
internal class RootedPath : IFileSystemPath
{
    private readonly string _value;

    private RootedPath(string path)
    {
        _value = path;
    }

    public static IFileSystemPath Create(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return EmptyPath.Instance;
        }

        return new RootedPath(path);
    }

    public static RootedPath CreateOrThrow(string path)
    {
        if (Create(path) is RootedPath rootedPath)
        {
            return rootedPath;
        }

        throw new ArgumentException($"Attempted to create {nameof(RootedPath)} from {path} but it was not rooted or it was an invalid path.");
    }

    public string GetPath()
    {
        return _value;
    }

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
