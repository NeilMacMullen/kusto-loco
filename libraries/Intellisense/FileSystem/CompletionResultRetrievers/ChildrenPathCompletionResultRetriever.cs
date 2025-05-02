using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

/// <summary>
/// Retrieves the children of a root or child directory ending with a directory separator.
/// </summary>
internal class ChildrenPathCompletionResultRetriever(IFileSystemReader reader, IShareReader shareReader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        var path = fileSystemPath.GetPath();
        if (!path.EndsWithDirectorySeparator())
        {
            return CompletionResult.Empty;
        }

        if (fileSystemPath is UncPath p && p.IsHost())
        {
            return shareReader.GetShares(p.Host).ToCompletionResult();
        }

        if (fileSystemPath.IsRootDirectory())
        {
            return reader.GetChildren(path).ToCompletionResult();
        }

        return reader
            .GetChildren(fileSystemPath.GetParent())
            .ToCompletionResult();
    }
}
