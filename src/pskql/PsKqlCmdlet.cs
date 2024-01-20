using System.Drawing.Text;
using System.Management.Automation;
using System.Web.Services.Description;
using BabyKusto.Core.Util;
using KustoSupport;
using NetTopologySuite.Index.HPRtree;

namespace pskql;

[Cmdlet("Query", "Kql")]
public class PsKqlCmdlet : Cmdlet
{
    private readonly List<PSObject> _objects = [];
    // Declare the parameters for the cmdlet.
    [Parameter(ValueFromPipeline = true)]
    public PSObject Item { get; set; } = new(string.Empty);

    [Parameter(Mandatory = true)]
    public string Query { get; set; } = string.Empty;


    protected override void ProcessRecord()
    {
        _objects.Add(Item);
     
    }

    protected override void EndProcessing()
    {
        var builder = TableBuilder.CreateEmpty("data", _objects.Count);
   
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
            {
                AddValue("Value",types.First(),() =>item.BaseObject,rowIndex);
            }
            else //complex types
            foreach (var p in item.Properties)
            {
                if (badProperties.Contains(p.Name))
                    continue;
                WriteDebug($"{rowIndex} Attempting to add property {p.Name} of type {p.TypeNameOfValue}...");
                try
                {
                    AddValue(p.Name, p.TypeNameOfValue,() => p.Value,rowIndex);
                }
                catch (Exception e)
                {
                    WriteWarning(e.Message);
                    badProperties.Add(p.Name);
                }
            }

            rowIndex++;
        }
        WriteDebug($"Creating context...");
        foreach (var name in columnNames)
        {
            var cb = columnBuilders[name];
            cb.PadTo(rowIndex);
            builder.WithColumn(name, cb.ToColumn());
        }

        var context = new KustoQueryContext();
        context.AddTable(builder.ToTableSource());
        WriteDebug("Running query...");
        var result = context.RunTabularQueryWithoutDemandBasedTableLoading(Query);
        if (result.Error.Length != 0)
        {
            WriteError(new ErrorRecord(new ArgumentException(result.Error),"QueryError",ErrorCategory.InvalidArgument,null));
        }
        else
        {
            WriteDebug("Emitting output...");
            var columns = result.ColumnDefinitions();
            foreach (var row in result.EnumerateRows())
            {

                var o = new PSObject();

                foreach (var k in columns)
                    o.Properties.Add(new PSVariableProperty(new PSVariable(k.Name, row[k.Index]))); 
                WriteObject(o);
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
        
        
        void AddValue(string columnName, string typeName, Func<object?> valueGetter,int rowIndex)
        {
            //Uses a getter func because evaluating some complex types can take a very long time
            switch (typeName)
            {
                case "System.String":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(string));
                    colBuilder.AddAt(valueGetter(),rowIndex);
                    break;
                }
                case "System.Int32":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(long));
                    colBuilder.AddAt(valueGetter(),rowIndex);
                    break;
                }
                case "System.Int64":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(long));
                    colBuilder.AddAt(valueGetter(),rowIndex);
                    break;
                }
                case "System.Boolean":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(bool));
                    colBuilder.AddAt(valueGetter(),rowIndex);
                    break;
                }
                case "System.DateTime":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(DateTime));
                    colBuilder.AddAt(valueGetter(),rowIndex);
                    break;
                }
                case "System.Guid":
                {
                    var colBuilder = GetOrAdd(columnName, typeof(Guid));
                    colBuilder.AddAt(valueGetter(),rowIndex);
                    break;
                }
                default:
                    //WriteObject($"{columnName} has unknown type {typeName}");
                    break;
            }
        }
    }
}