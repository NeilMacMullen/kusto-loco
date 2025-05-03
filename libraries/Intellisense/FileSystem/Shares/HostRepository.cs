namespace Intellisense.FileSystem.Shares;

// TODO: EF Core + SQLite?
internal interface IHostRepository
{
    IReadOnlyList<string> List();
    void Add(string host);
    void Remove(string host);
}

internal class HostRepository : IHostRepository
{
    private readonly HashSet<string> _hosts = new([], StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> List() => _hosts.ToList();

    public void Add(string host) => _hosts.Add(host);

    public void Remove(string host) => _hosts.Remove(host);
}
