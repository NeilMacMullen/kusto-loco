using System.IO;
using System.Text.Json;
using NetTopologySuite.Index.Quadtree;
using NotNullStrings;

namespace lokqlDx;

public class Workspace
{
    public string WorkingDirectory { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class WorkspaceManager
{
    public string _path =string.Empty;
    public Workspace workspace = new();


    private void EnsureWorkspacePopulated()
    {
        if (!workspace.WorkingDirectory.IsBlank()) return;
        workspace.WorkingDirectory = Environment.CurrentDirectory;
        workspace.Text=@"
# move the cursor over a block of lines and press CTRL-ENTER to run
# commands prefixed with '.' are special commands.  Use .help  to list

.help 

# loads a CSV file into a table called 'data'

.load c:\data\mydata.csv data

# gets the distribution of values in the 'Name' column

data 
| summarize count() by Name 
| render barchart


";
    }


    public WorkspaceManager()
    {
    }

    public void Save(Workspace workspace)
    {
        try
        {
            var json = JsonSerializer.Serialize(workspace);
            File.WriteAllText(_path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving workspace: {e.Message}");
        }
    }

    public void Load(string path)
    {
        _path = path;
        try
        {
            var json = File.ReadAllText(_path);
            workspace = JsonSerializer.Deserialize<Workspace>(json)!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading workspace: {e.Message}");
        }
        EnsureWorkspacePopulated();
    }
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