namespace lokqlDx;

public class MruEntry
{
    public string Path { get; set; } = string.Empty;
    public DateTime LastAccessed { get; set; } = DateTime.MinValue;
}
/// <summary>
/// The list of recently used files/projects
/// </summary>
/// <remarks>
/// This may be quite contended if the user has multiple copies of the application
/// open
/// </remarks>
public class PersistedMruList
{
    public MruEntry [] RecentProjects { get; set; } = [];
}

/// <summary>
/// User preferences that affect the UI
/// </summary>
/// <remarks>
/// These are persisted to disk on application close and loaded at startup
/// </remarks>
public class UIPreferences
{
    public double FontSize { get; set; } = 20;
    public string FontFamily { get; set; } = "Consolas";
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public double WindowTop { get; set; }
    public double WindowLeft { get; set; }
    public bool WordWrap { get; set; } = false;
    public bool ShowLineNumbers { get; set; } = false;
    public string[] MainGridSerialization { get; set; } = [];
    public string[] EditorGridSerialization { get; set; } = [];
}

