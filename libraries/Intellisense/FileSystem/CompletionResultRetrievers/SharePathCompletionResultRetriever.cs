using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SharePathCompletionResultRetriever(IShareReader shareReader)
    : IFileSystemPathCompletionResultRetriever
{

    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath is not UncPath p)
        {
            return CompletionResult.Empty;
        }

        var path = p.GetPath();

        if (p.IsHost() && path.EndsWithDirectorySeparator())
        {
            return (await shareReader.GetSharesAsync(p.Host)).ToCompletionResult();
        }

        if (p.IsShare() && !path.EndsWithDirectorySeparator())
        {
            return (await shareReader.GetSharesAsync(p.Host)).ToCompletionResult() with
            {
                Filter = p.Share
            };
        }

        return CompletionResult.Empty;

    }
}
