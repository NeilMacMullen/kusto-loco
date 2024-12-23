using System.IO;
using System.Text.Json;
using NotNullStrings;

namespace lokqlDx;

public class PreferencesManager
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    public Preferences Preferences { get; private set; } = new();

    public static void EnsureDefaultFolderExists()
    {
        Directory.CreateDirectory(RootPath());
    }

    private static string RootPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lokql");
    }

    private static string PreferencesPath()
    {
        return Path.Combine(RootPath(), "preferences.json");
    }

    private static string DefaultWorkspacePath()
    {
        return Path.Combine(RootPath(), Path.ChangeExtension("default",WorkspaceManager.Extension));
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
            if (Preferences.FontSize==0)
                Preferences.FontSize = 12;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading preferences: {e.Message}");
        }

        if (Preferences.FontFamily.IsBlank())
            Preferences.FontFamily = "Consolas";
        if (Preferences.LastWorkspacePath.IsBlank()) Preferences.LastWorkspacePath = DefaultWorkspacePath();
    }
}
