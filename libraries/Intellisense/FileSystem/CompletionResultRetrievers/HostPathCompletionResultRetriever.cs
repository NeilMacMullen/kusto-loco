using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class HostPathCompletionResultRetriever(IHostRepository hostRepository)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath.GetPath() is "//" or @"\\")
        {
            return (await hostRepository.ListAsync()).ToCompletionResult();
        }

        if (fileSystemPath is UncPath p && p.IsHost() && !p.GetPath().EndsWithDirectorySeparator())
        {
            return (await hostRepository.ListAsync()).ToCompletionResult() with
            {
                Filter = p.Host
            };
        }

        return CompletionResult.Empty;
    }
}
