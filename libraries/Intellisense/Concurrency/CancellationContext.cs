using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Intellisense.Concurrency;

/// <summary>
/// Wrapper around a cancellation token for use in scoped requests.
/// </summary>
internal class CancellationContext : IDisposable
{
    public CancellationTokenSource TokenSource { get; private set; } = new();

    public void LinkToken(CancellationToken token) =>
        TokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, TokenSource.Token);

    public void Dispose() => TokenSource.Dispose();
}

internal static class CancellationContextServiceCollectionExtensions
{
    public static void AddCancellationContext(this IServiceCollection services)
    {
        services.TryAddScoped<CancellationContext>();
        services.TryAddScoped(x => x.GetRequiredService<CancellationContext>().TokenSource);
    }
}
