using System.Collections;
using System.IO;
using System.Text.Json;
using NotNullStrings;

namespace lokqlDx;

public class PreferencesManager
{
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    public UIPreferences UIPreferences { get; private set; } = new();
  

    public static void EnsureDefaultFolderExists()
    {
        Directory.CreateDirectory(RootPath());
    }

    private static string RootPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lokql");
    }

    private static string PreferencesPath(string filename)
    {
        return Path.Combine(RootPath(), $"{filename}.json");
    }

    private static string DefaultWorkspacePath()
    {
        return Path.Combine(RootPath(), Path.ChangeExtension("default",WorkspaceManager.Extension));
    }

    private bool Save<T>(string fileName,T contents)
    {
        try
        {
           EnsureDefaultFolderExists();
            var json = JsonSerializer.Serialize(contents, _options);
            File.WriteAllText(PreferencesPath(fileName), json);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving preferences: {e.Message}");
        }

        return false;
    }

    private T Load<T>(string fileName, T fallback)
    {
        try
        {
           EnsureDefaultFolderExists();
            var json = File.ReadAllText(PreferencesPath(fileName));
            return JsonSerializer.Deserialize<T>(json, _options) ?? fallback;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading preferences: {e.Message}");
        }

        return fallback;
    }

    private const string UIPreferencesFileName = "ui";
    private const string ApplicationPreferencesFileName = "preferences";
    private const string MruFileName = "mru";

    public void Save(UIPreferences preferences)
    {
        Save(UIPreferencesFileName, preferences);
    }
    public void SaveMru()
    {
        Save(MruFileName, new PersistedMruList());
    }
    public void Save(ApplicationPreferences preferences)
    {
        Save(ApplicationPreferencesFileName, preferences);
    }

    public ApplicationPreferences FetchApplicationPreferencesFromDisk()
    {
        return  Load(ApplicationPreferencesFileName, new ApplicationPreferences());
    }

    public void RetrieveUiPreferencesFromDisk()
    {
        UIPreferences = Load(UIPreferencesFileName, new UIPreferences());
    }
    public void UpdateMru()
    {
        var mru = Load(MruFileName, new PersistedMruList());
    }

    public void SaveUiPrefs()
    {
        Save(UIPreferences);
    }

    public string[] GetMruItems()
    {
        var mru = Load(MruFileName, new PersistedMruList());
        return mru.RecentProjects.
            OrderByDescending(i=>i.LastAccessed)
            .Select(p => p.Path)
            .ToArray();

    }

    public void BringToTopOfMruList(string path)
    {
        var mru = Load(MruFileName, new PersistedMruList());
        var resorted= mru.RecentProjects
            .Where(i => i.Path != path)
            .Concat([new MruEntry { Path = path, LastAccessed = DateTime.Now }])
            .OrderByDescending(i => i.LastAccessed)
            .Take(100)
            .ToArray();
        Save(MruFileName, new PersistedMruList { RecentProjects = resorted });
    }
}
