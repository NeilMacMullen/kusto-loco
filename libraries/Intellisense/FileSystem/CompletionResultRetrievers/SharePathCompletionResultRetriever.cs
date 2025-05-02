using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SharePathCompletionResultRetriever(IShareReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath is not UncPath path
            || !path.IsHost()
            || !path.GetPath().EndsWithDirectorySeparator()
           )
        {
            return CompletionResult.Empty;
        }

        return reader
            .GetShares(path.Host)
            .ToCompletionResult();
    }
}
