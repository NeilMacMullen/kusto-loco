using NotNullStrings;

namespace Intellisense.FileSystem.Paths;

public readonly record struct ParentChildPathPair
{
    public string ParentPath { get; }
    public string CurrentPath { get; }

    private static readonly ParentChildPathPair Empty = new(string.Empty, string.Empty);

    private ParentChildPathPair(string parentPath, string currentPath)
    {
        ParentPath = parentPath;
        CurrentPath = currentPath;
    }

    public static ParentChildPathPair Create(string path)
    {
        var dirName = Path.GetDirectoryName(path).NullToEmpty();
        var fileName = Path.GetFileName(path);

        if (dirName.IsBlank() || fileName.IsBlank())
        {
            return Empty;
        }

        return new ParentChildPathPair(dirName, fileName);
    }
}
