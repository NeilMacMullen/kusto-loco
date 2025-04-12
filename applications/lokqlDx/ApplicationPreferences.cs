namespace lokqlDx;

/// <summary>
/// Global Preferences that are applied for every workspace
/// </summary>
public class ApplicationPreferences
{
    public bool AutoSave { get; set; }
    public string StartupScript { get; set; }= string.Empty;
    public bool HasShownLanding { get; set; }
}
