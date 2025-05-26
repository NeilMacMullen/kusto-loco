namespace LokqlDx.Models;

public class RecentWorkspace(string name, string path)
{
    public string Name { get; } = name;
    public string Path { get; } = path;
}
