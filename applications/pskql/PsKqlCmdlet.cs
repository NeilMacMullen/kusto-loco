using System.Diagnostics;
using System.Management.Automation;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using KustoLoco.Rendering.ScottPlot;

namespace pskql;

[Cmdlet(VerbsData.Edit, "Kql")]
public class PsKqlCmdlet : Cmdlet
{
    private const string TableName = "data";

    private readonly Dictionary<string, BaseColumnBuilder> _columnBuilders = [];
    private readonly List<string> _columnNames = [];
    private readonly List<PSObject> _objects = [];
    private readonly List<string> addedProperties = [];

    private readonly List<string> badProperties = [];

    private TimeSpan PropertyTimeout = TimeSpan.FromMilliseconds(10);

    // Declare the parameters for the cmdlet.
    [Parameter(ValueFromPipeline = true)] public PSObject Item { get; set; } = new(string.Empty);

    [Parameter(Position = 0, HelpMessage = "KQL query string fragment. Default value is 'getschema'")]
    public string Query { get; set; } = "getschema";

    [Parameter(HelpMessage = "Skip PSScriptProperty members (may avoid some issues)")]
    public SwitchParameter SkipScriptProperties { get; set; }

    [Parameter(HelpMessage = "Skip PSScriptProperty members (may avoid some issues)")]
    public int Timeout { get; set; } = 10;

    [Parameter(HelpMessage =
        "Queries are usually implicitly prefixed with 'data |' but this can be disabled with this switch"
    )]
    public SwitchParameter NoQueryPrefix { get; set; }

    protected override void ProcessRecord() => _objects.Add(Item);

    private void AddPropertyInfo(string prefix, PSPropertyInfo p, int rowIndex)
    {
        var timer = Stopwatch.StartNew();
        var pName = prefix + p.Name;
        WriteDebug($"AddPropertyInfo {pName}");


        if (badProperties.Contains(pName))
            return;
        try
        {
            switch (p)
            {
                case PSProperty psProperty:
                    break;
                case PSAliasProperty psAlias:
                    break;
                case PSCodeProperty psCode:
                    break;
                case PSScriptProperty psScript:
                    if (SkipScriptProperties)
                        return;
                    break;
                case PSNoteProperty psNote:
                    break;

                default:
                    //we deliberately skip complex properties for now
                    //in the future we might decide to flatten them or import has JsonNodes
                    WriteDebug(
                        $"{rowIndex} Skipping property {pName} of unsupported type {p.TypeNameOfValue} {p.GetType().Name}...");
                    badProperties.Add(pName);
                    return;
            }

            var pTypeNameOfValue = p.TypeNameOfValue;
            var pValue = p.Value;
            //it's important we check the property type _before_ attempting to access the Value
            //since some Values are extremely expensive to access
            //TODO we currently assume non-primitive Values are expensive
            //but we could be more sophisticated here and attempt to time accesses
            WriteDebug(
                $"name:{rowIndex} {pName} valueTypeName:{pTypeNameOfValue} pType:{p.GetType().Name} valType:{pValue?.GetType().Name ?? "null"}");
            // it's possible that not all rows have the same properties, for
            //example if we've done an 'ls' and have a mix of files and directories
            //therefore we have to be careful to insert cells at the appropriate row
            //index and pad with nulls where necessary
            if (!addedProperties.Contains(pName))
                WriteDebug($"{rowIndex} Attempting to add property {pName} of type {pTypeNameOfValue}...");


            AddValue(pName, pTypeNameOfValue, pValue, rowIndex);
            addedProperties.Add(pName);
            if (timer.Elapsed > PropertyTimeout)
                badProperties.Add(pName);
        }
        catch (Exception e)
        {
            WriteDebug($"Unable to get property {pName}");
            WriteDebug(e.Message);
            badProperties.Add(pName);
        }
        finally
        {
            WriteDebug($"AddPropertyInfo {pName} time {timer.Elapsed}");
        }
    }

    //complex types
    private void AddObject(PSObject item, int rowIndex)
    {
        foreach (var p in item.Properties) AddPropertyInfo(string.Empty, p, rowIndex);
    }

    private static bool IsSimpleType(string typeName) => TypeNameHelper.GetTypeFromName(typeName) != typeof(object);

    protected override void EndProcessing()
    {
        var builder = TableBuilder.CreateEmpty(TableName, _objects.Count);
        PropertyTimeout = TimeSpan.FromMilliseconds(Timeout);
        WriteDebug($"Adding {_objects.Count} items");
        var rowIndex = 0;
        foreach (var item in _objects)
        {
            var types = item.TypeNames.ToArray();
            //simple types
            if (IsSimpleType(types.First()))
                AddValue("Value", types.First(), item.BaseObject, rowIndex);
            else AddObject(item, rowIndex);
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
        var result = context.RunQueryWithoutDemandBasedTableLoading(query);
        if (result.Error.Length != 0)
        {
            WriteError(new ErrorRecord(new ArgumentException(result.Error), "QueryError", ErrorCategory.InvalidArgument,
                null));
        }
        else
        {
            WriteDebug("Emitting output...");
            if (!result.IsChart)
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
                var str = ScottPlotKustoResultRenderer.RenderToSixelWithPad(result,
                    new KustoSettingsProvider(), 3);
                WriteObject(str);
            }
        }
    }


    private BaseColumnBuilder GetOrCreateBuilder(string name, string typeName)
    {
        var type = TypeNameHelper.GetTypeFromName(typeName);
        if (_columnBuilders.TryGetValue(name, out var b))
            return b;
        b = ColumnHelpers.CreateBuilder(type, string.Empty);
        _columnBuilders[name] = b;
        _columnNames.Add(name);
        return b;
    }


    private void AddValue(string columnName, string typeName, object? value, int rowIndex)
    {
        if (typeName == TypeNameHelper.TypeName<object>())
        {
            if (value != null)
            {
                WriteDebug($"property '{columnName}'  is object so trying to derive type from value");
                typeName = value.GetType().ToString();
                var isPsObject = value is PSObject;
                WriteDebug($"prop {columnName} typeof '{value}' is {typeName} IsObject ? {isPsObject}");


                if (value is PSObject ps)
                {
                    if (ps.BaseObject is null)
                    {
                        value = "null";
                        return;
                    }

                    var baseType = ps.BaseObject.GetType();

                    typeName = baseType.ToString() ?? "null";
                    WriteDebug($"prop {columnName} baseobj type {typeName}");

                    if (baseType.IsEnum)
                    {
                        WriteDebug($"{typeName} is enum");
                        value = ps.BaseObject!.ToString();
                    }
                    else if (IsSimpleType(typeName))
                    {
                        WriteDebug($"{typeName} is known type");
                        value = ps.BaseObject;
                    }
                    else
                    {
                        WriteDebug($"prop {columnName} baseobj typeof '{ps.BaseObject}' is {typeName}");
                        WriteDebug($"property '{columnName}'  is PSObject with properties so trying to add props");
                        foreach (var info in ps.Properties)
                            AddPropertyInfo(columnName + "_", info, rowIndex);
                        return;
                    }
                }
            }
            else
            {
                typeName = TypeNameHelper.TypeName<string>();
                value = value?.ToString() ?? string.Empty;
            }
        }

        if (!IsSimpleType(typeName))
        {
            WriteDebug($"{columnName} has type {typeName} val {value} .. enumerating properties");
            typeName = TypeNameHelper.TypeName<string>();
            value = value?.ToString() ?? string.Empty;
        }

        //WriteDebug($"Getting builder for {columnName}");
        var colBuilder = GetOrCreateBuilder(columnName, typeName);
        colBuilder.AddAt(value, rowIndex);
    }
}
