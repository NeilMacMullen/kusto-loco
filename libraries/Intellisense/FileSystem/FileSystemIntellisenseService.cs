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
    Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path);
}

internal class FileSystemIntellisenseService(
    ILogger<IFileSystemIntellisenseService> logger,
    IPathFactory pathFactory,
    IEnumerable<IFileSystemPathCompletionResultRetriever> retrievers
)
    : IFileSystemIntellisenseService
{
    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path)
    {

        try
        {
            var pathObj = pathFactory.Create(path);

            var result = await retrievers
                .Select(x => x.GetCompletionResultAsync(pathObj))
                .ToList()
                .ToWhenAnyAsyncEnumerable()
                .Where(x => !x.IsEmpty())
                .Take(1)
                .ToListAsync();

            if (result.Count is 0)
            {
                return CompletionResult.Empty;
            }

            return result[0];
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error occurred while fetching intellisense results. Returning empty result.");
            return CompletionResult.Empty;
        }

    }
}

file static class AsyncExtensions
{
    public static async IAsyncEnumerable<T> ToWhenAnyAsyncEnumerable<T>(this IList<Task<T>> tasks)
    {
        while (tasks.Count > 0)
        {
            var next = await Task.WhenAny(tasks);
            tasks.Remove(next);
            yield return await next;
        }
    }
}
