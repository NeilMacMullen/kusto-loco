using System.Diagnostics;
using System.Management.Automation;
using BabyKusto.Core.Evaluation;
using BabyKusto.Core.Util;
using KustoSupport;

namespace pskql;

[Cmdlet(VerbsData.Edit, "Kql")]
public class PsKqlCmdlet : Cmdlet
{
    private const string TableName = "data";
    private readonly List<PSObject> _objects = [];
    private bool _noQueryPrefix;

    // Declare the parameters for the cmdlet.
    [Parameter(ValueFromPipeline = true)] public PSObject Item { get; set; } = new(string.Empty);

    [Parameter(Position = 0, HelpMessage = "KQL query string fragment. Default value is 'getschema'")]
    public string Query { get; set; } = "getschema";

    [Parameter(HelpMessage =
        "Queries are usually implicitly prefixed with 'data |' but this can be disabled with this switch"
    )]
    public SwitchParameter NoQueryPrefix
    {
        get => _noQueryPrefix;
        set => _noQueryPrefix = value;
    }

    protected override void ProcessRecord()
    {
        _objects.Add(Item);
    }

    protected override void EndProcessing()
    {
        var builder = TableBuilder.CreateEmpty(TableName, _objects.Count);

        var columnBuilders = new Dictionary<string, BaseColumnBuilder>();
        var columnNames = new List<string>();
        var badProperties = new List<string>();
        WriteDebug($"Adding {_objects.Count} items");
        var rowIndex = 0;
        foreach (var item in _objects)
        {
            var types = item.TypeNames.ToArray();
            //simple types
            if (types.First() == "System.String")
                AddValue("Value", types.First(), () => item.BaseObject, rowIndex);
            else //complex types
                foreach (var p in item.Properties)
                {
                    if (badProperties.Contains(p.Name))
                        continue;
                    WriteDebug($"{rowIndex} Attempting to add property {p.Name} of type {p.TypeNameOfValue}...");
                    try
                    {
                        AddValue(p.Name, p.TypeNameOfValue, () => p.Value, rowIndex);
                    }
                    catch (Exception e)
                    {
                        WriteWarning(e.Message);
                        badProperties.Add(p.Name);
                    }
                }

            rowIndex++;
        }

        WriteDebug("Creating context...");
        foreach (var name in columnNames)
        {
            var cb = columnBuilders[name];
            cb.PadTo(rowIndex);
            builder.WithColumn(name, cb.ToColumn());
        }

        var context = new KustoQueryContext();
        context.AddTable(builder.ToTableSource());
        WriteDebug("Running query...");
        var query = NoQueryPrefix ? Query : $"data | {Query}";
        var result = context.RunTabularQueryWithoutDemandBasedTableLoading(query);
        if (result.Error.Length != 0)
        {
            WriteError(new ErrorRecord(new ArgumentException(result.Error), "QueryError", ErrorCategory.InvalidArgument,
                null));
        }
        else
        {
            WriteDebug("Emitting output...");
            if (result.Visualization == VisualizationState.Empty)
            {
                var columns = result.ColumnDefinitions();
                foreach (var row in result.EnumerateRows())
                {
                    var o = new PSObject();

                    foreach (var k in columns)
                        o.Properties.Add(new PSVariableProperty(new PSVariable(k.Name, row[k.Index])));
                    WriteObject(o);
                }
            }
            else
            {
                var html = KustoResultRenderer.RenderToHtml(result);
                var filename = Path.ChangeExtension(Path.GetTempFileName(), ".html");
                File.WriteAllText(filename, html);
                Process.Start(new ProcessStartInfo { FileName = filename, UseShellExecute = true });
            }
        }

        return;

        BaseColumnBuilder GetOrAdd(string name, Type type)
        {
            if (columnBuilders!.TryGetValue(name, out var b))
                return b;
            b = ColumnHelpers.CreateBuilder(type);
            columnBuilders[name] = b;
            columnNames.Add(name);
            return b;
        }


        void AddValue(string columnName, string typeName, Func<object?> valueGetter, int rowIndex)
        {
            //Uses a getter func because evaluating some complex types can take a very long time
            switch (typeName)
            {
                case "System.String":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(string));
                    colBuilder.AddAt(valueGetter(), rowIndex);
                    break;
                }
                case "System.Int32":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(long));
                    colBuilder.AddAt(valueGetter(), rowIndex);
                    break;
                }
                case "System.Int64":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(long));
                    colBuilder.AddAt(valueGetter(), rowIndex);
                    break;
                }
                case "System.Boolean":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(bool));
                    colBuilder.AddAt(valueGetter(), rowIndex);
                    break;
                }
                case "System.DateTime":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(DateTime));
                    colBuilder.AddAt(valueGetter(), rowIndex);
                    break;
                }
                case "System.Guid":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(Guid));
                    colBuilder.AddAt(valueGetter(), rowIndex);
                    break;
                }
            }
        }
    }
}