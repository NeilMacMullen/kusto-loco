using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

/// <summary>
/// Retrieves the siblings of a directory or file and adds filtering data based on the name.
/// </summary>
internal class SiblingPathCompletionResultRetriever(IFileSystemReader reader) : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        var pair = ParentChildPathPair.Create(fileSystemPath.GetPath());

        return reader
                .GetChildren(pair.ParentPath)
                .ToCompletionResult() with
            {
                Filter = pair.CurrentPath
            };
    }

    public Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath) =>
        Task.FromResult(GetCompletionResult(fileSystemPath));
}
