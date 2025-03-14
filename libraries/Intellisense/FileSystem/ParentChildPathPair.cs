namespace Intellisense.FileSystem;

internal record struct ParentChildPathPair
{
    public string ParentPath { get; }
    public string CurrentPath { get; }

    private ParentChildPathPair(string parentPath, string currentPath)
    {
        ParentPath = parentPath;
        CurrentPath = currentPath;
    }

    public static ParentChildPathPair? Create(string path)
    {
        if (path.GetNonEmptyParentDirectory() is not { } dirName)
        {
            return null;
        }

        if (path.GetNonEmptyFileName() is not { } fileName)
        {
            return null;
        }

        return new ParentChildPathPair(dirName, fileName);
    }

    public static ParentChildPathPair CreateOrThrow(string path)
    {
        if (Create(path) is not { } pair)
        {
            throw new ArgumentException($"Expected {path} to have a nonempty parent directory but it did not.");
        }

        return pair;
    }
}
