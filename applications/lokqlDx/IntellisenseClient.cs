using Intellisense;
using Intellisense.FileSystem;

namespace lokqlDx;

public class IntellisenseClient(IFileSystemIntellisenseService service)
{
    private CancellationTokenSource _cts = new();

    public async Task<CompletionResult> GetPathIntellisenseOptionsAsync(string path)
    {
        var cts = new CancellationTokenSource();
        var prevCts = Interlocked.Exchange(ref _cts, cts);
        await prevCts.CancelAsync();

        var res = await service.GetPathIntellisenseOptionsAsync(path, cts.Token);
        cts.Token.ThrowIfCancellationRequested();
        return res;
    }

    public async Task CancelPendingRequests()
    {
        var cts = new CancellationTokenSource();
        var prevCts = Interlocked.Exchange(ref _cts, cts);
        await prevCts.CancelAsync();
    }

}
