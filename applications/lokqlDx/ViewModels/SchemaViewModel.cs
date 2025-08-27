using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Dock.Model.Mvvm.Controls;
using Lokql.Engine;
using NotNullStrings;

namespace LokqlDx.ViewModels;

public partial class SchemaViewModel : LokqlTool
{
    [ObservableProperty] private string _filter = string.Empty;
    private ObservableCollection<Schema> _schema = [];
    private SchemaLine[] _schemaLine = [];

    [ObservableProperty] private HierarchicalTreeDataGridSource<Schema> _schemaSource = new([]);

    [ObservableProperty] private bool _showCommands;

    public SchemaViewModel()
    {
        Title = "Schema";
        Messaging.RegisterForValue<SchemaUpdateMessage, SchemaLine[]>(this,Update);
    }

    [RelayCommand]
    public void FilterChanged() => Update(_schemaLine);

    private void Update(SchemaLine[] schema)
    {
        _schemaLine = schema;
        var s = schema
            .Where(ApplyFilter)
            .GroupBy(s => new { s.Command, s.Table })
            .Select(g => new Schema
            {
                Depth = "Command",
                Command = g.Key.Command,
                TableName = g.Key.Table,
                Children = g.Select(s => new Schema
                {
                    Depth = "Table",
                    TableName = g.Key.Table,
                    ColumnName = s.Column,
                    Type = s.Type
                }).ToArray()
            })
            .OrderBy(t => t.Command)
            .ToArray();

        _schema = new ObservableCollection<Schema>(s);

        var cols = new ColumnList<Schema>();
        if (ShowCommands)
            cols.Add(new TextColumn<Schema, string>("Command", x => x.Command));

        cols.AddRange(new ColumnList<Schema>
        {
            new HierarchicalExpanderColumn<Schema>(
                new TextColumn<Schema, string>("Table", x => x.TableName),
                x => x.Children),
            new TextColumn<Schema, string>("Column", x => x.ColumnName),
            new TextColumn<Schema, string>("Type", x => x.Type)
        });

        SchemaSource =
            ShowCommands
                ? new HierarchicalTreeDataGridSource<Schema>(_schema)
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
                }
                : new HierarchicalTreeDataGridSource<Schema>(_schema)
                {
                    Columns =
                    {
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
        if (!ShowCommands && arg.Command.IsNotBlank())
            return false;
        var argStr = $"{arg.Command}  {arg.Table} {arg.Column}";
        var toks = Filter.Tokenize(" ");
        if (!toks.Any())
            return true;
        return toks.All(t => argStr.Contains(t, StringComparison.InvariantCultureIgnoreCase));
    }

    public void ExpandAllNodes()
    {
        void ExpandChildren(IReadOnlyList<Schema> nodes, IndexPath parentPath)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var path = parentPath.Append(i);
                SchemaSource.Expand(path);
            }
        }

        ExpandChildren(SchemaSource.Items.ToArray(), new IndexPath());
    }

    [RelayCommand]
    public void DoubleClick(SchemaClick click) =>
       Messaging.Send(new InsertTextMessage(click.ClickedText));

    partial void OnShowCommandsChanged(bool value) =>
        // Call your command or update logic here
        FilterChanged();
}

public readonly record struct SchemaClick(Schema Schema, string ClickedText);

public class Schema
{
    public Schema[] Children = [];
    public string ColumnName = string.Empty;
    public string Command = string.Empty;
    public string Depth = "";
    public string TableName = string.Empty;
    public string Type = string.Empty;
}
