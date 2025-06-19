using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

public class SharePathCompletionResultRetriever(IShareService shareService)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetSiblingsAsync(FileSystemPath path)
    {
        if (path is not UncPath { IsOnlyHostAndShare: true } p) return CompletionResult.Empty;

        return (await shareService.GetSharesAsync(p.OriginalHost)).ToCompletionResult() with
        {
            Filter = p.Share
        };
    }

    public async Task<CompletionResult> GetChildrenAsync(FileSystemPath path) =>
        path is not UncPath { IsOnlyHost: true } p
            ? CompletionResult.Empty
            : (await shareService.GetSharesAsync(p.OriginalHost)).ToCompletionResult();
}
