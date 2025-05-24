namespace Intellisense.Concurrency;

public class ExclusiveRequestSession
{
    private CancellationTokenSource _cts = new();

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> fn)
    {
        var cts = Cancel();
        // don't block main thread
        var result = await Task.Run(() => fn(cts.Token));
        cts.Token.ThrowIfCancellationRequested();
        return result;
    }

    private CancellationTokenSource Cancel()
    {
        var cts = new CancellationTokenSource();
        var prevCts = Interlocked.Exchange(ref _cts, cts);
        prevCts.Cancel();
        return cts;
    }

    public async Task CancelRequestAsync() => await _cts.CancelAsync();
}
