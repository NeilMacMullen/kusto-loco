using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

/// <summary>
/// Retrieves the children of a root or child directory ending with a directory separator.
/// </summary>
internal class ChildrenPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        var dir = GetTargetDirectory(fileSystemPath);

        return reader
            .GetChildren(dir)
            .ToCompletionResult();
    }

    private static string GetTargetDirectory(IFileSystemPath fileSystemPath)
    {
        var path = fileSystemPath.GetPath();
        if (!path.EndsWithDirectorySeparator())
        {
            return string.Empty;
        }

        if (fileSystemPath.IsRootDirectory())
        {
            return path;
        }

        return Path.GetDirectoryName(path) ?? string.Empty;

    }
}
