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
            return _value.Length is 1 || IsUncRootDirectory();
        }

        if (_value.Length is 2)
        {
            return false;
        }

        return _value.Length is 3 && _value[2].IsDirectorySeparator();
    }

    private bool IsUncRootDirectory()
    {
        if (!Uri.TryCreate(_value, UriKind.Absolute, out var uri) || !uri.IsUnc)
        {
            return false;
        }

        // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-dtyp/62e862f4-2a51-452e-8eeb-dc4ff5ee33cc
        // also note that .net normalizes slashes

        // "//host/c:/" => "c:/"
        var uriPath = uri.GetComponents(UriComponents.Path, UriFormat.Unescaped);

        if (!uriPath[0].IsValidDriveChar())
        {
            return false;
        }

        if (uriPath.Length is 1)
        {
            return true;
        }

        if (uriPath.Length is 2)
        {
            return uriPath[1] is '$' || uriPath[1].IsDirectorySeparator();
        }

        return uriPath.Length is 3
               && (uriPath[1] is '$' || uriPath[1] == Path.VolumeSeparatorChar)
               && uriPath[2].IsDirectorySeparator();
    }
}
