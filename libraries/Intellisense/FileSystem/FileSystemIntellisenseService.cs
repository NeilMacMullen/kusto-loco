using System.Runtime.CompilerServices;
using Intellisense.Configuration;
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
    IOptions<IntellisenseOptions> timeoutOptions
)
    : IFileSystemIntellisenseService
{
    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(
        string path,
        CancellationToken cancelToken = default
    )
    {
        using var scope = scopeFactory.CreateScope();
        var retrievers =
            scope.ServiceProvider.GetRequiredService<IEnumerable<IFileSystemPathCompletionResultRetriever>>();
        using var ctx = scope.ServiceProvider.GetRequiredService<CancellationContext>();
        ctx.TokenSource.CancelAfter(timeoutOptions.Value.Timeout);
        ctx.LinkToken(cancelToken);
        using var _ = logger.BeginScope(new()
            {
                [nameof(path)] = path,
                [nameof(timeoutOptions.Value.Timeout)] = timeoutOptions.Value.Timeout,
            }
        );
        var token = ctx.TokenSource.Token;

        try
        {
            var pathObj = pathFactory.Create(path);

            var result = await retrievers
                .Select(x => x.GetCompletionResultAsync(pathObj))
                .WhenEach(token)
                .Where(x => !x.IsEmpty())
                .FirstOrDefaultAsync(token);

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
    public static async IAsyncEnumerable<T> WhenEach<T>(this IEnumerable<Task<T>> tasks, [EnumeratorCancellation] CancellationToken token)
    {
        ConfiguredCancelableAsyncEnumerable<Task<T>> enumerator = Task.WhenEach(tasks).WithCancellation(token);

        await foreach (Task<T> item in enumerator)
        {
            yield return await item;

        }
    }
}
