namespace Intellisense.FileSystem.Shares;

internal interface IShareReader
{
    public IEnumerable<string> GetShares(string host);
}