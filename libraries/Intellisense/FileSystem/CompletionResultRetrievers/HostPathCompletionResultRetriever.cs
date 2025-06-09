using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

public class HostPathCompletionResultRetriever(IShareService shareService)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetSiblingsAsync(FileSystemPath path)
    {
        if (path is not UncPath { IsOnlyHost: true } p)
        {
            return CompletionResult.Empty;
        }

        return (await shareService.GetHostsAsync()).ToCompletionResult() with
        {
            Filter = p.OriginalHost
        };
    }

    public async Task<CompletionResult> GetChildrenAsync(FileSystemPath path)
    {
        if (path.Value is not ("//" or @"\\"))
        {
            return CompletionResult.Empty;
        }

        return (await shareService.GetHostsAsync()).ToCompletionResult();
    }
}
