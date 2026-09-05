using System.Text.Json;
using Avalonia.Media;

namespace LokqlDx;

public class PreferencesManager
{
    private const string UIPreferencesFileName = "ui";
    private const string ApplicationPreferencesFileName = "preferences";
    private const string MruFileName = "mru";
    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
    private ApplicationPreferences _cachedApplicationPreferences = new();
    public UIPreferences UIPreferences { get; private set; } = new();

    public void EnsureDefaultFolderExists() => Directory.CreateDirectory(RootPath());

    private static string RootPath() =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "lokql");

    private static string PreferencesPath(string filename) => Path.Combine(RootPath(), $"{filename}.json");

    private static string DefaultWorkspacePath() =>
        Path.Combine(RootPath(), Path.ChangeExtension("default", WorkspaceManager.Extension));

    private bool Save<T>(string fileName, T contents)
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

    public void Save(UIPreferences preferences) => Save(UIPreferencesFileName, preferences);

    public void SaveMru() => Save(MruFileName, new PersistedMruList());

    public void Save(ApplicationPreferences preferences)
    {
        Save(ApplicationPreferencesFileName, preferences);
        _cachedApplicationPreferences = preferences;
    }

    public ApplicationPreferences FetchApplicationPreferencesFromDisk()
    {
        _cachedApplicationPreferences = Load(ApplicationPreferencesFileName, new ApplicationPreferences());
        return _cachedApplicationPreferences;
    }

    public void RetrieveUiPreferencesFromDisk()
    {
        UIPreferences = Load(UIPreferencesFileName, new UIPreferences());
        var fonts = FontManager.Current.SystemFonts
            .OrderBy(x => x.Name)
            .ToList();

        // in case Consolas is not found, or user uninstalls a font they installed
        var font = fonts.FirstOrDefault(f =>
                       f.Name == UIPreferences.FontFamily ||
                       f.Name == "Consolas" ||
                       f.Name == "SF Mono" || f.Name == "Menlo" || f.Name == "Monaco" || // macOS defaults
                       f.Name == "Noto Sans Mono" || f.Name == "DejaVu Sans Mono" || // some linux distros defaults
                       f.Name.Contains("Mono", StringComparison.OrdinalIgnoreCase))
                   ?? PreferencesHelper.GetFirstMonospaceFontFamily()
                   ?? FontManager.Current.DefaultFontFamily;

        UIPreferences.FontFamily = font.Name;
    }

    public void UpdateMru()
    {
        var mru = Load(MruFileName, new PersistedMruList());
    }

    public void SaveUiPrefs() => Save(UIPreferences);

    public string[] GetMruItems()
    {
        return GetMruEntries()
            .Select(p => p.Path)
            .ToArray();
    }

    /// <summary>
    ///     Returns the recently used workspaces with pinned items first, then most-recently accessed
    /// </summary>
    public MruEntry[] GetMruEntries()
    {
        var mru = Load(MruFileName, new PersistedMruList());
        return SortEntries(mru.RecentProjects);
    }

    private static MruEntry[] SortEntries(IEnumerable<MruEntry> entries) =>
        entries.OrderByDescending(i => i.IsPinned)
            .ThenByDescending(i => i.LastAccessed)
            .ToArray();

    public void BringToTopOfMruList(string path) => BringToTopOfMruList(path, string.Empty);

    public void BringToTopOfMruList(string path, string description)
    {
        var mru = Load(MruFileName, new PersistedMruList());
        var existing = mru.RecentProjects.FirstOrDefault(i => i.Path == path);
        var entry = new MruEntry
        {
            Path = path,
            Name = Path.GetFileNameWithoutExtension(path),
            Description = description,
            IsPinned = existing?.IsPinned ?? false,
            LastAccessed = DateTime.Now
        };
        var resorted = SortEntries(mru.RecentProjects
                .Where(i => i.Path != path)
                .Concat([entry]))
            .Take(100)
            .ToArray();
        Save(MruFileName, new PersistedMruList { RecentProjects = resorted });
    }

    public void RemoveFromMruList(string path)
    {
        var mru = Load(MruFileName, new PersistedMruList());
        var remaining = mru.RecentProjects
            .Where(i => i.Path != path)
            .ToArray();
        Save(MruFileName, new PersistedMruList { RecentProjects = remaining });
    }

    public void SetPinned(string path, bool pinned)
    {
        var mru = Load(MruFileName, new PersistedMruList());
        foreach (var entry in mru.RecentProjects.Where(i => i.Path == path))
            entry.IsPinned = pinned;
        Save(MruFileName, new PersistedMruList { RecentProjects = SortEntries(mru.RecentProjects) });
    }

    /// <summary>
    ///     Lightweight method to fetch the cached application settings
    /// </summary>
    /// <remarks>
    ///     Most of the time we want to ensure we have the latest settings from disk
    ///     but some things are lightweight enough that we can just use a slightly stale
    ///     version
    /// </remarks>
    public ApplicationPreferences FetchCachedApplicationSettings() => _cachedApplicationPreferences;
}
