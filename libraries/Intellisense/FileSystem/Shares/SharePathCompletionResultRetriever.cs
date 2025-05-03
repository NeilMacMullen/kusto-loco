using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.Shares;

internal class SharePathCompletionResultRetriever(IShareReader shareReader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath is not UncPath p)
        {
            return CompletionResult.Empty;
        }

        var path = p.GetPath();

        if (p.IsHost() && path.EndsWithDirectorySeparator())
        {
            return shareReader.GetShares(p.Host).ToCompletionResult();
        }

        if (p.IsShare() && !path.EndsWithDirectorySeparator())
        {
            return shareReader.GetShares(p.Host).ToCompletionResult() with
            {
                Filter = p.Share
            };
        }

        return CompletionResult.Empty;
    }
}
