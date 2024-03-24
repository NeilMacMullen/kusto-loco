using System.Management.Automation;
using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Util;
using KustoLoco.Rendering;

namespace pskql;

[Cmdlet(VerbsData.Edit, "Kql")]
public class PsKqlCmdlet : Cmdlet
{
    private const string TableName = "data";

    private readonly Dictionary<string, BaseColumnBuilder> _columnBuilders = [];
    private readonly List<string> _columnNames = [];
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


        var badProperties = new List<string>();
        var addedProperties = new List<string>();
        WriteDebug($"Adding {_objects.Count} items");
        var rowIndex = 0;
        foreach (var item in _objects)
        {
            var types = item.TypeNames.ToArray();
            //simple types
            if (types.First() == TypeNameHelper.TypeName<string>())
                AddValue("Value", types.First(), item.BaseObject, rowIndex);
            else //complex types
            {
                // it's possible that not all rows have the same properties, for
                //example if we've done an 'ls' and have a mix of files and directories
                //therefore we have to be careful to insert cells at the appropriate row
                //index and pad with nulls where necessary
                foreach (var p in item.Properties)
                {
                    if (badProperties.Contains(p.Name))
                        continue;
                    //it's important we check the property type _before_ attempting to access the Value
                    //since some Values are extremely expensive to access
                    //TODO we currently assume non-primitive Values are expensive
                    //but we could be more sophisticated here and attempt to time accesses
                    if (TypeNameHelper.GetTypeFromName(p.TypeNameOfValue) == typeof(object)
                        && p.TypeNameOfValue != TypeNameHelper.TypeName<object>())
                    {
                        //we deliberately skip complex properties for now
                        //in the future we might decide to flatten them or import has JsonNodes
                        WriteDebug($"{rowIndex} Skipping property {p.Name} of unsupported type {p.TypeNameOfValue}...");
                        badProperties.Add(p.Name);
                        continue;
                    }

                    if (!addedProperties.Contains(p.Name))
                    {
                        WriteDebug($"{rowIndex} Attempting to add property {p.Name} of type {p.TypeNameOfValue}...");
                    }

                    try
                    {
                        AddValue(p.Name, p.TypeNameOfValue, p.Value, rowIndex);
                        addedProperties.Add(p.Name);
                    }
                    catch (Exception e)
                    {
                        WriteDebug(e.Message);
                        badProperties.Add(p.Name);
                    }
                }
            }

            rowIndex++;
        }

        WriteDebug("Creating context...");
        foreach (var name in _columnNames)
        {
            var cb = _columnBuilders[name];
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
                KustoResultRenderer.RenderChartInBrowser(result);
            }
        }
    }

    private BaseColumnBuilder GetOrCreateBuilder(string name, string typeName)
    {
        var type = TypeNameHelper.GetTypeFromName(typeName);
        if (_columnBuilders!.TryGetValue(name, out var b))
            return b;
        b = ColumnHelpers.CreateBuilder(type);
        _columnBuilders[name] = b;
        _columnNames.Add(name);
        return b;
    }


    private void AddValue(string columnName, string typeName, object? value, int rowIndex)
    {
        //special-casing for properties of type "object" which we turn into strings for the purpose of querying
        if (typeName == TypeNameHelper.TypeName<object>())
        {
            typeName = TypeNameHelper.TypeName<string>();
            value = value?.ToString() ?? string.Empty;
        }


        var colBuilder = GetOrCreateBuilder(columnName, typeName);
        colBuilder.AddAt(value, rowIndex);
    }
}