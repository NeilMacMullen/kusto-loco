namespace Intellisense;

internal class CancellationContext : IDisposable
{
    public CancellationTokenSource TokenSource { get; private set; } = new();
    public void LinkToken(CancellationToken token)
    {
        var prev = TokenSource;
        TokenSource = CancellationTokenSource.CreateLinkedTokenSource(token,prev.Token);
    }

    public void Dispose() => TokenSource.Dispose();
}
