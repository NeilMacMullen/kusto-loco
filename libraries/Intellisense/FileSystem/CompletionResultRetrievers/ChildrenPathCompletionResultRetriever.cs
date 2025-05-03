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
        var path = fileSystemPath.GetPath();
        if (!path.EndsWithDirectorySeparator())
        {
            return CompletionResult.Empty;
        }

        if (fileSystemPath.IsRootDirectory())
        {
            return reader.GetChildren(path).ToCompletionResult();
        }

        return reader
            .GetChildren(fileSystemPath.GetParent())
            .ToCompletionResult();
    }

    public Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath) => Task.FromResult(GetCompletionResult(fileSystemPath));
}
