namespace Intellisense.FileSystem.Shares;

public interface IShareService
{
    Task<IEnumerable<string>> GetSharesAsync(string host);
    Task<IEnumerable<string>> GetHostsAsync();
}
