using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    /// <summary>
    /// Retrieves intellisense completion results from a given path.
    /// </summary>
    /// <returns>
    /// An empty completion result if the path is invalid, does not exist, or does not have any children.
    /// </returns>
    Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path);
}

public class FileSystemIntellisenseService(
    IPathFactory pathFactory,
    IEnumerable<IFileSystemPathCompletionResultRetriever> retrievers
)
    : IFileSystemIntellisenseService
{
    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(
        string path
    )
    {
        var pathObj = pathFactory.Create(path);

        var tasks = pathObj.EndsWithDirectorySeparator
            ? retrievers.Select(x => x.GetChildrenAsync(pathObj))
            : retrievers.Select(x => x.GetSiblingsAsync(pathObj));

        return await Task
            .WhenEach(tasks)
#if NET10_0_OR_GREATER
            .Select(async (Task<CompletionResult> x, CancellationToken _) => await x)
#else
            .SelectAwait(async x => await x)
#endif
            .Where(x => !x.IsEmpty())
            .FirstOrDefaultAsync() ?? CompletionResult.Empty;
    }
}
