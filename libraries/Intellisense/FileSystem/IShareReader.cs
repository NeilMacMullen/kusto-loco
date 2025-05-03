namespace Intellisense.FileSystem;

internal interface IShareReader
{
    public IEnumerable<string> GetShares(string host);
}