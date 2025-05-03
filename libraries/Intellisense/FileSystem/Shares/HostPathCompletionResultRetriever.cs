using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.Shares;

internal class HostPathCompletionResultRetriever(IHostRepository hostRepository)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath.GetPath() is "//" or @"\\")
        {
            return hostRepository.List().ToCompletionResult();
        }


        return CompletionResult.Empty;
    }
}
