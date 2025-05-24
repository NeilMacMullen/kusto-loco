namespace Intellisense.FileSystem.Shares;

public class NullShareService : IShareService
{
    private static readonly Task<IEnumerable<string>> Empty = Task.FromResult(Enumerable.Empty<string>());
    public Task<IEnumerable<string>> GetSharesAsync(string host) => Empty;
    public Task<IEnumerable<string>> GetHostsAsync() => Empty;
}
