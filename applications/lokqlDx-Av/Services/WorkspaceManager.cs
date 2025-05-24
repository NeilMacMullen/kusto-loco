using System.Collections;
using System.IO;
using System.Text.Json;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using NotNullStrings;

namespace LokqlDx.Services;

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
    private Workspace _workspace = new();
    public string Path = string.Empty;
    public Workspace Workspace => _workspace;

    public static string GlobPattern => $"*.{Extension}";

    public bool IsNewWorkspace => Path.IsBlank();
    public KustoSettingsProvider Settings { get; } = new();
    private JsonSerializerOptions _options = new() { WriteIndented = true };
    private void EnsureWorkspacePopulated()
    {
        var UserText = _workspace.Text;
        if (!UserText.IsBlank()) return;
        UserText = @"
# move the cursor over a block of lines and press SHIFT-ENTER to run

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

    public void Save(string path, Workspace workspace)
    {
        Path = path;
        try
        {
            var json = JsonSerializer.Serialize(workspace,_options);
            File.WriteAllText(Path, json);
            _workspace = workspace;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error saving workspace: {e.Message}");
        }
    }

    public void Load(string path)
    {
        if (path.IsBlank())
        {
            CreateNew();
            return;
        }

        var rootSettingFolderPath =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "kustoloco");

        if (!Directory.Exists(rootSettingFolderPath))
            Directory.CreateDirectory(rootSettingFolderPath);

        _workspace = new Workspace();
        ResetSettings();
        Path = path;
        SetWorkingPaths();
        if (path.IsNotBlank())
            try
            {
                var json = File.ReadAllText(Path);
                _workspace = JsonSerializer.Deserialize<Workspace>(json)!;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error loading workspace: {e.Message}");
            }

        EnsureWorkspacePopulated();
    }


    private void ResetSettings()
    {
        Settings.Reset();
        //now add in settings from env...
        var env = Environment.GetEnvironmentVariables();
        foreach (DictionaryEntry  e in env)
        {
            var v = e.Value?.ToString();
            if (v is null) continue;
            Settings.Set($"env.{e.Key}",v);
        }
        if (Path.IsNotBlank())
            Settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, System.IO.Path.GetDirectoryName(Path)!);
    }
    public void CreateNew()
    {
        _workspace = new Workspace();
        ResetSettings();
        Path = string.Empty;
    }

    public bool IsDirty(Workspace wkspc)
    {
        return wkspc != _workspace;
    }

    public string ContainingFolder()
    {
        return System.IO.Path.GetDirectoryName(Path).NullToEmpty();
    }

    public void SetWorkingPaths()
    {
        if (Path.IsBlank())
            return;
        var containingFolder = ContainingFolder();
        Settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, containingFolder);
        Settings.Set(LokqlSettings.ScriptPath.Name, containingFolder);
        Settings.Set(LokqlSettings.QueryPath.Name, containingFolder);
    }
}
