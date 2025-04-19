using Intellisense.FileSystem.CompletionResultRetrievers;
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

internal class FileSystemIntellisenseService : IFileSystemIntellisenseService
{
    private readonly IFileSystemPathCompletionResultRetriever[] _retrievers;
    private readonly ILogger<IFileSystemIntellisenseService> _logger;
    private readonly IRootedPathFactory _rootedPathFactory;

    public FileSystemIntellisenseService(IFileSystemReader reader, ILogger<IFileSystemIntellisenseService> logger, IRootedPathFactory rootedPathFactory)
    {
        _rootedPathFactory = rootedPathFactory;
        _logger = logger;
        _retrievers =
        [
            new ChildrenPathCompletionResultRetriever(reader),
            new SiblingPathCompletionResultRetriever(reader)
        ];
    }

    public CompletionResult GetPathIntellisenseOptions(string path)
    {

        try
        {
            // Only rooted paths supported at this time

            var rootedPath = _rootedPathFactory.Create(path);

            return _retrievers
                .Select(x => x.GetCompletionResult(rootedPath))
                .FirstOrDefault(x => !x.IsEmpty(),CompletionResult.Empty);

        }
        catch (IOException e)
        {
            _logger.LogError(e,"IO error occurred while fetching intellisense results. Returning empty result.");
            return CompletionResult.Empty;
        }
    }
}
