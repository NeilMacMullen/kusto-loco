using Intellisense.Concurrency;
using Intellisense.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Intellisense;

public interface IIntellisenseService
{
    Task<CompletionResult> GetCompletionResultAsync(string input, CancellationToken cancellationToken);
}

internal class IntellisenseService(IServiceScopeFactory scopeFactory, ILogger<IntellisenseClient> logger) : IIntellisenseService
{
    public async Task<CompletionResult> GetCompletionResultAsync(string input, CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope(new() { [nameof(input)] = input });

        using var scope = scopeFactory.CreateScope();
        using var ctx = scope.ServiceProvider.GetRequiredService<CancellationContext>();
        ctx.LinkToken(cancellationToken);


        var fsService = scope.ServiceProvider.GetRequiredService<IFileSystemIntellisenseService>();
        var result = await fsService.GetPathIntellisenseOptionsAsync(input);
        logger.LogTrace("Received {@CompletionResult}", result);
        return result;
    }
}