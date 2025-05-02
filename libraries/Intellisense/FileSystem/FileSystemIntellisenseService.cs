using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.Logging;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    /// <summary>
    /// Retrieves intellisense completion results from a given path.
    /// </summary>
    /// <returns>
    /// An empty completion result if the path is invalid, does not exist, or does not have any children.
    /// </returns>
    CompletionResult GetPathIntellisenseOptions(string path);
}

internal class FileSystemIntellisenseService(
    ILogger<IFileSystemIntellisenseService> logger,
    IPathFactory pathFactory,
    IEnumerable<IFileSystemPathCompletionResultRetriever> retrievers
    )
    : IFileSystemIntellisenseService
{

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        try
        {
            var pathObj = pathFactory.Create(path);

            return retrievers
                .Select(x => x.GetCompletionResult(pathObj))
                .FirstOrDefault(x => !x.IsEmpty(), CompletionResult.Empty);
        }
        catch (IOException e)
        {
            logger.LogError(e, "IO error occurred while fetching intellisense results. Returning empty result.");
            return CompletionResult.Empty;
        }
    }
}
