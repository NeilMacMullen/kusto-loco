using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

public class LocalFileSystemCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public Task<CompletionResult> GetSiblingsAsync(FileSystemPath path)
    {
        var pair = ParentChildPathPair.Create(path.Value);

        var result = reader
                .GetChildren(pair.ParentPath)
                .ToCompletionResult() with
            {
                Filter = pair.CurrentPath
            };

        return Task.FromResult(result);
    }

    public Task<CompletionResult> GetChildrenAsync(FileSystemPath path)
    {
        var input = path.IsRootDirectory ? path.Value : path.ParentPath;

        var result = reader.GetChildren(input).ToCompletionResult();

        return Task.FromResult(result);
    }
}
