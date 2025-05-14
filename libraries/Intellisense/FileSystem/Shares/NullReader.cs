namespace Intellisense.FileSystem.Shares;

public class NullReader : IHostReader, IShareReader
{
    public Task<IEnumerable<string>> GetHostsAsync() => Task.FromResult(Enumerable.Empty<string>());

    public Task<IEnumerable<string>> GetSharesAsync(string host) => Task.FromResult(Enumerable.Empty<string>());
}
