using System.IO;
using System.Text.Json;
using KustoLoco.Core.Settings;
using NotNullStrings;

namespace lokqlDx;

/// <summary>
///     A WorkspaceManager is responsible for loading and saving a workspace.
/// </summary>
/// <remarks>
///     If the desired workspace can't be loaded we supply a default
///     workspace that contains basic instructions
/// </remarks>
public class WorkspaceManager
{
    public const string Extension = "lokql";
    public string Path = string.Empty;


    public WorkspaceManager(KustoSettingsProvider settings)
    {
        Settings = settings;
    }

    public static string GlobPattern => $"*.{Extension}";

    public string UserText { get; private set; } = string.Empty;
    public KustoSettingsProvider Settings { get; }

    private void EnsureWorkspacePopulated()
    {
        if (!UserText.IsBlank()) return;
        UserText = @"
# move the cursor over a block of lines and press CTRL-ENTER to run

# load a CSV file into a table called 'data'

.load c:\data\mydata.csv data

# gets the distribution of values in the 'Name' column

data 
| summarize count() by Name 
| render barchart

#save the results to a parquet file
.save namecount.parqet

";
    }

    public void Save(string path, string userText)
    {
        UserText = userText;
        Path = path;
        var workspace = new Workspace
        {
            Text = UserText,
            Settings = Settings.Enumerate().ToDictionary(kv => kv.Name, kv => kv.Value)
        };
       
        try
        {
            var json = JsonSerializer.Serialize(workspace);
            File.WriteAllText(Path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving workspace: {e.Message}");
        }
    }

    public void Load(string path)
    {
        if (!File.Exists(path))
        {
            var rootSettingFolderPath =
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "kustoloco");
            if (!Directory.Exists(rootSettingFolderPath))
                Directory.CreateDirectory(rootSettingFolderPath);

            path = System.IO.Path.Combine(rootSettingFolderPath, "settings");
            File.WriteAllText(path, JsonSerializer.Serialize(new Workspace()));
        }

        Path = path;
        try
        {
            var json = File.ReadAllText(Path);
            var workspace = JsonSerializer.Deserialize<Workspace>(json)!;
            UserText = workspace.Text;
            Settings.Reset();
            foreach (var kv in workspace.Settings) Settings.Set(kv.Key, kv.Value);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading workspace: {e.Message}");
        }

        EnsureWorkspacePopulated();
    }

    public void CreateNewInCurrentFolder()
    {
        UserText = string.Empty;
        Settings.Reset();
        Path = System.IO.Path.Combine(ContainingFolder(),
            System.IO.Path.ChangeExtension("new", Extension));
    }

    public string ContainingFolder()
    {
        return System.IO.Path.GetDirectoryName(Path).NullToEmpty();
    }
}
