namespace Intellisense.Concurrency;

/// <summary>
/// Wrapper around a cancellation token for use in scoped requests.
/// </summary>
public class CancellationContext : IDisposable
{
    public CancellationTokenSource TokenSource { get; private set; } = new();

    public void LinkToken(CancellationToken token) =>
        TokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, TokenSource.Token);

    public void Dispose() => TokenSource.Dispose();
}