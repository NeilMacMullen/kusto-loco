using System.IO;
using System.Text.Json;
using NetTopologySuite.Index.Quadtree;
using NotNullStrings;

namespace lokqlDx;

/// <summary>
/// A Workspace is the query text and working directory for a user's session.
/// </summary>
public class Workspace
{
    public string WorkingDirectory { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class Preferences
{
    public string LastWorkspacePath { get; set; } = string.Empty;
    public double FontSize { get; set; }
    public string FontFamily { get; set; } = string.Empty;
}

public class PreferencesManager
{

    public static void EnsureDefaultFolderExists()
    {
        Directory.CreateDirectory(RootPath());
    }
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    public Preferences Preferences { get; private set; } = new();

    private static string RootPath()
    {
        return System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lokql");
    }
    private static string PreferencesPath()
    {
        return System.IO.Path.Combine(RootPath(), "preferences.json");
    }

    private static string DefaultWorkspacePath()
    {
        return System.IO.Path.Combine(RootPath(), "defaultWorkspace.json");
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(RootPath());
            var json = JsonSerializer.Serialize(Preferences, _options);
            File.WriteAllText(PreferencesPath(), json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving preferences: {e.Message}");
        }
    }

    public void Load()
    {
        try
        {
            var json = File.ReadAllText(PreferencesPath());
            Preferences = JsonSerializer.Deserialize<Preferences>(json)!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading preferences: {e.Message}");
        }

        if (Preferences.LastWorkspacePath.IsBlank())
        {
            Preferences.LastWorkspacePath = DefaultWorkspacePath();
        }
    }
}
