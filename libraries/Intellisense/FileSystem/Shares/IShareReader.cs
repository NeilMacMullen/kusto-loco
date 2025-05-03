namespace Intellisense.FileSystem.Shares;

internal interface IShareReader
{
    public Task<IEnumerable<string>> GetSharesAsync(string host);
}