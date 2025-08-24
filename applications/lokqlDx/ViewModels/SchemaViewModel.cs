using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Mvvm.Controls;
using Lokql.Engine;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2010.CustomUI;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class SchemaViewModel : Tool, INotifyPropertyChanged
{
    private ObservableCollection<Schema> _schema=[];
    private SchemaLine[] _schemaLine=[];
    public SchemaViewModel(DisplayPreferencesViewModel displayPreferencesPreferences)
    {
        Title = "Schema";
        CanClose = false;
    }

    [ObservableProperty] private HierarchicalTreeDataGridSource<Schema> _schemaSource = new HierarchicalTreeDataGridSource<Schema>([]);

    [RelayCommand]
    public void FilterChanged()
    {
        Update(_schemaLine);
    }
    
    [ObservableProperty] private string _filter=string.Empty;
    public void Update(SchemaLine[] schema)
    {
        _schemaLine=schema;
        var s = schema
            .Where(ApplyFilter)
            .GroupBy(s=>new {s.Command,s.Table})
            .Select(g=>new Schema()
            {
                Command =g.Key.Command,
                TableName = g.Key.Table,
                Children = g.Select(s=>new Schema()
                {
                    TableName = g.Key.Table,
                    ColumnName = s.Column,
                    Type = s.Type
                }).ToArray()
            })
            .OrderBy(t=>t.Command)
            .ToArray(); 

        _schema = new ObservableCollection<Schema>(s);
        SchemaSource = new HierarchicalTreeDataGridSource<Schema>(_schema)
        {
            Columns =
            {
                new TextColumn<Schema, string>("Command", x => x.Command),
                new HierarchicalExpanderColumn<Schema>(
                    new TextColumn<Schema, string>("Table", x => x.TableName),
                    x => x.Children),
                new TextColumn<Schema, string>("Column", x => x.ColumnName),
                new TextColumn<Schema, string>("Type", x => x.Type)
            }
        };

       ExpandAllNodes();
    }

    private bool ApplyFilter(SchemaLine arg)
    {
        var argStr = $"{arg.Command}  {arg.Table} {arg.Column}";
        var toks = Filter.Tokenize(" ");
        if (!toks.Any())
            return true;
        return toks.All(t=>argStr.Contains(t, StringComparison.InvariantCultureIgnoreCase));
    }

    public void ExpandAllNodes()
    {
        void ExpandChildren(IReadOnlyList<Schema> nodes, IndexPath parentPath)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var path = parentPath.Append(i);
                SchemaSource.Expand(path);
            }
        }

        ExpandChildren(SchemaSource.Items.ToArray(), new IndexPath());
    }

    [RelayCommand]
    public void DoubleClick(SchemaClick click)

    {
        Console.WriteLine("here");
    }

}

public readonly record struct SchemaClick(Schema Schema, string ClickedText);
public class Schema
{
    public Schema[] Children = [];
    public string Command = string.Empty;
    public string ColumnName = string.Empty;
    public string TableName = String.Empty;
    public string Type = String.Empty;
}
