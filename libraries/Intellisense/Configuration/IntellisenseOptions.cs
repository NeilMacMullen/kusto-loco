namespace Intellisense.Configuration;

public class IntellisenseOptions
{
    /// <summary>
    /// The directory for intellisense storage data.
    /// </summary>
    public string Directory { get; set; } = string.Empty;
    /// <summary>
    /// The maximum amount of time an intellisense request should take.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    private const string FileName = "hosts.json";
    internal string DatabaseLocation => Path.Combine(Directory, FileName);
    internal string DatabaseConnectionString => $"Data Source={DatabaseLocation};";
}
