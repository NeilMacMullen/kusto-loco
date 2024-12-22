namespace lokqlDx;

public class Preferences
{
    public string LastWorkspacePath { get; set; } = string.Empty;
    public double FontSize { get; set; } = 20;
    public string FontFamily { get; set; } = string.Empty;
    public double WindowWidth { get; set; }
    public double WindowHeight { get; set; }
    public double WindowTop { get; set; }
    public double WindowLeft { get; set; }
    public string StartupScript { get; set; }= string.Empty;
    public string[] RecentProjects { get; set; } = [];
}
