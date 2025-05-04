namespace Intellisense.FileSystem.Shares;

internal interface IShareResource : IDisposable
{
    Task<List<ShareInfo>> GetSharesAsync(string host, CancellationToken token);
}