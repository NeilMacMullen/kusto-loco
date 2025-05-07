using Intellisense.FileSystem.Paths;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    /// <summary>
    /// Retrieves intellisense completion results from a given path.
    /// </summary>
    /// <returns>
    /// An empty completion result if the path is invalid, does not exist, or does not have any children.
    /// </returns>
    Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path, CancellationToken cancelToken = default);
}

internal class FileSystemIntellisenseService(
    ILogger<IFileSystemIntellisenseService> logger,
    IPathFactory pathFactory,
    IServiceScopeFactory scopeFactory,
    IOptions<IntellisenseTimeoutOptions> timeoutOptions
)
    : IFileSystemIntellisenseService
{

    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path, CancellationToken cancelToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var retrievers =
            scope.ServiceProvider.GetRequiredService<IEnumerable<IFileSystemPathCompletionResultRetriever>>();
        using var ctx = scope.ServiceProvider.GetRequiredService<CancellationContext>();
        ctx.TokenSource.CancelAfter(timeoutOptions.Value.IntellisenseTimeout);
        ctx.LinkToken(cancelToken);
        using var _ = logger.BeginScope(new()
            {
                [nameof(path)] = path,
                [nameof(timeoutOptions.Value.IntellisenseTimeout)] = timeoutOptions.Value.IntellisenseTimeout,
            }
        );
        var token = ctx.TokenSource.Token;

        try
        {
            var pathObj = pathFactory.Create(path);

            var result = await retrievers
                .Select(x => x.GetCompletionResultAsync(pathObj))
                .ToList()
                .AsWhenAnyAsyncEnumerable()
                .TakeWhile(_ => !token.IsCancellationRequested)
                .Where(x => !x.IsEmpty())
                .FirstOrDefaultAsync(ctx.TokenSource.Token);

            return result ?? CompletionResult.Empty;
        }
        catch (OperationCanceledException e)
        {
            logger.LogTrace(e, "Cancelled or timed out while fetching intellisense results.");
            throw;
        }
        catch (IOException e)
        {
            logger.LogError(e, "IO Error occurred while fetching intellisense results. Returning empty result.");
            return CompletionResult.Empty;
        }
    }
}

file static class AsyncExtensions
{
    public static async IAsyncEnumerable<T> AsWhenAnyAsyncEnumerable<T>(this IList<Task<T>> tasks)
    {
        while (tasks.Count > 0)
        {
            var next = await Task.WhenAny(tasks);
            tasks.Remove(next);
            yield return await next;
        }
    }
}
