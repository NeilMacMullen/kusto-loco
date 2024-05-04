using System.IO;
using System.Text.Json;
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
    public string _path =string.Empty;
    public Workspace workspace = new();


    private void EnsureWorkspacePopulated()
    {
        if (!workspace.WorkingDirectory.IsBlank()) return;
        workspace.WorkingDirectory = @"C:\kustoloco";
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

    public void Save(Workspace workspace,string path)
    {
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
            workspace = JsonSerializer.Deserialize<Workspace>(json)!;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading workspace: {e.Message}");
        }
        EnsureWorkspacePopulated();
    }
}
