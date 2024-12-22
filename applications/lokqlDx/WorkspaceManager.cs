using System.IO;
using System.Text.Json;
using KustoLoco.Core.Settings;
using Lokql.Engine;
using NotNullStrings;
using ZstdSharp.Unsafe;

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
    private Workspace _workspace=new Workspace();
    public Workspace Workspace => _workspace;

    public static string GlobPattern => $"*.{Extension}";

  
    public KustoSettingsProvider Settings { get; } = new KustoSettingsProvider();

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

    public void Save(string path,Workspace workspace)
    {
       
        Path = path;
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

       _workspace=new Workspace();
        Settings.Reset();
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

    public void CreateNew()
    {
        _workspace= new Workspace();
        Settings.Reset();
        Path = string.Empty;
    }

    public bool IsDirty(Workspace wkspc) => wkspc!=_workspace;
    public string ContainingFolder() => System.IO.Path.GetDirectoryName(Path).NullToEmpty();

    public void SetWorkingPaths()
    {
        if(Path.IsBlank())
            return;
        var containingFolder = ContainingFolder();
        Settings.Set(StandardFormatAdaptor.Settings.KustoDataPath.Name, containingFolder);
        Settings.Set(LokqlSettings.ScriptPath.Name, containingFolder);
        Settings.Set(LokqlSettings.QueryPath.Name, containingFolder);
    }

}
