using System.Collections.Concurrent;
using NotNullStrings;

namespace Intellisense.FileSystem.Shares;

internal interface IHostRepository
{
    Task<IEnumerable<string>> ListAsync();
    Task AddAsync(string host);
}

internal class HostRepository(TimeProvider timeProvider) : IHostRepository
{
    private readonly ConcurrentDictionary<string, HostName> _hosts = new([]);

    public Task<IEnumerable<string>> ListAsync() => Task.FromResult<IEnumerable<string>>(_hosts.Keys);

    public async Task AddAsync(string host)
    {
        await Task.CompletedTask;
        var hostName = CreateHostName(host);

        if (!IsValid(hostName))
        {
            return;
        }

        _hosts.TryAdd(hostName.Name, hostName);
    }

    private HostName CreateHostName(string host)
    {
        var hostName = new HostName
        {
            Name = host.ToLowerInvariant(),
            LastUpdated = timeProvider.GetUtcNow()
        };
        return hostName;
    }

    private bool IsValid(HostName hostName) =>
        hostName.Name.IsNotBlank()
        && hostName.Name.IsLowercase()
        && hostName.LastUpdated <= timeProvider.GetUtcNow();
}

// hosts are probably resolved in a case insensitive manner regardless of any context and/or OS platform this class might be used in?
// we'll just have everything be lowercase and not worry about this
// keep this in mind when migrating over to db / file storage
internal class HostName
{
    public int Id { get; set; }

    // index unique
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset LastUpdated { get; set; }
}