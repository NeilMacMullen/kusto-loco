using System.IO;
using System.Text.Json;
using KustoLoco.FileFormats;
using NotNullStrings;

namespace lokqlDx;

/// <summary>
/// A WorkspaceManager is responsible for loading and saving a workspace.
/// </summary>
/// <remarks>
/// If the desired workspace can't be loaded we supply a default
/// workspace that contains basic instructions
/// </remarks>
public class WorkspaceManager
{

    public const string Extension = "lokql";
    public static string GlobPattern=> $"*.{Extension}";
    public string _path =string.Empty;
   
    public string UserText { get; private set; } = string.Empty;

    public KustoSettings Settings{ get;private set;} = new KustoSettings();

    private void EnsureWorkspacePopulated()
    {
        if (!UserText.IsBlank()) return;
        UserText=@"
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

    public void Save(string path,string userText,KustoSettings settings)
    {
        var workspace = new Workspace
        {
            Text = userText,
            Settings = settings.Enumerate().ToDictionary(kv=>kv.Name,kv=>kv.Value)
        };
        _path = path;
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
            var workspace = JsonSerializer.Deserialize<Workspace>(json)!;
            UserText = workspace.Text;
            Settings = new KustoSettings();
            foreach (var kv in workspace.Settings)
            {
                Settings.Set(kv.Key, kv.Value);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading workspace: {e.Message}");
        }
        EnsureWorkspacePopulated();
    }

    public void CreateNewPathInCurrentFolder()
    {
       _path = Path.Combine(ContainingFolder(),
           Path.ChangeExtension("new",Extension));
    }

    public string ContainingFolder() =>Path.GetDirectoryName(_path).NullToEmpty();
}
