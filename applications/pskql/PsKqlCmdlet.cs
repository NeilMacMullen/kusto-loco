using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using KustoLoco.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Management.Automation;
using System.Runtime.InteropServices;
using KustoLoco.Rendering.ScottPlot;

using ScottPlot;
using Image = SixLabors.ImageSharp.Image;
using ImageFormat = ScottPlot.ImageFormat;
using Rectangle = System.Drawing.Rectangle;

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

    // Declare the parameters for the cmdlet.
    [Parameter(ValueFromPipeline = true)] public PSObject Item { get; set; } = new(string.Empty);

    [Parameter(Position = 0, HelpMessage = "KQL query string fragment. Default value is 'getschema'")]
    public string Query { get; set; } = "getschema";

    [Parameter(HelpMessage = "Evaluate PSScriptProperty members (may cause slow operation)")]
    public SwitchParameter EvaluateScriptProperties { get; set; }

    [Parameter(HelpMessage =
        "Queries are usually implicitly prefixed with 'data |' but this can be disabled with this switch"
    )]
    public SwitchParameter NoQueryPrefix { get; set; }

    protected override void ProcessRecord()
    {
        _objects.Add(Item);
    }


    private void AddPropertyInfo(string prefix, PSPropertyInfo p, int rowIndex)
    {
        var timer = Stopwatch.StartNew();
        var pName = prefix + p.Name;
        WriteDebug($"AddPropertyInfo {pName}");


        if (badProperties.Contains(pName))
            return;
        try
        {
            /*  if (p is PSScriptProperty)
              {
                  WriteDebug("Returning because script");
                  return;
              }
            */
            switch (p)
            {
                case PSProperty psProperty:
                    break;
                case PSAliasProperty psAlias:
                    break;
                case PSCodeProperty psCode:
                    break;
                case PSScriptProperty psScript:
                    if (!EvaluateScriptProperties)
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
                $"name:{rowIndex} {pName} valueTypeName:{pTypeNameOfValue} pType:{p.GetType().Name} valType:{pValue?.GetType()?.Name ?? "null"}");
            // it's possible that not all rows have the same properties, for
            //example if we've done an 'ls' and have a mix of files and directories
            //therefore we have to be careful to insert cells at the appropriate row
            //index and pad with nulls where necessary
            if (!addedProperties.Contains(pName))
                WriteDebug($"{rowIndex} Attempting to add property {pName} of type {pTypeNameOfValue}...");


            AddValue(pName, pTypeNameOfValue, pValue, rowIndex);
            addedProperties.Add(pName);
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

    private static bool IsSimpleType(string typeName)
    {
        return TypeNameHelper.GetTypeFromName(typeName) != typeof(object);
    }
    
    protected override void EndProcessing()
    {
        var builder = TableBuilder.CreateEmpty(TableName, _objects.Count);

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
                ScottPlot.Plot lot = new();
                GenericScottPlotter.Render(lot, result);
                GenericScottPlotter.UseDarkMode(lot);
                lot.Title(result.Visualization.PropertyOr("title", DateTime.UtcNow.ToShortTimeString()));
              
                var bytes = lot.GetImageBytes(800,800,ImageFormat.);

                using MemoryStream strm = new(bytes);
              var image = Image.Load<Rgba32>(strm);
              var str = Sixel.ImageToSixel(image, 256, 80);
              WriteObject(str);
            }
        }
    }

   
    private BaseColumnBuilder GetOrCreateBuilder(string name, string typeName)
    {
        var type = TypeNameHelper.GetTypeFromName(typeName);
        if (_columnBuilders!.TryGetValue(name, out var b))
            return b;
        b = ColumnHelpers.CreateBuilder(type, String.Empty);
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
                WriteDebug($"prop {columnName} typeof '{value}' is {typeName}");


                if (value is PSObject ps)
                {
                    typeName = ps.BaseObject?.GetType()?.ToString() ?? "null";
                    if (IsSimpleType(typeName))
                    {
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


internal class VTWriter : IDisposable
{
    private readonly TextWriter? _writer = null;
    private readonly FileStream? _windowsStream = null;
    private readonly bool _customwriter = false;
    private bool _disposed;

    public VTWriter()
    {
        bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        bool isRedirected = Console.IsOutputRedirected;
#if NET472
    if (isWindows && !isRedirected)
    {
      _windowsStream = new FileStream(NativeMethods.OpenConOut(), FileAccess.Write);
      _writer = new StreamWriter(_windowsStream);
      _customwriter = true;
    }
#else
        if (isWindows && !isRedirected)
        {
            // Open the Windows stream to CONOUT$, for better performance..
            // Console.Write is too slow for gifs.
            _windowsStream = File.OpenWrite("CONOUT$");
            _writer = new StreamWriter(_windowsStream);
            _customwriter = true;
        }
#endif
    }

    public void Write(string text)
    {
        if (_customwriter)
        {
            _writer?.Write(text);
        }
        else
        {
            Console.Write(text);
        }
    }

    public void WriteLine(string text)
    {
        if (_customwriter)
        {
            _writer?.WriteLine(text);
        }
        else
        {
            Console.WriteLine(text);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && _customwriter)
        {
            if (disposing)
            {
                _writer?.Dispose();
                _windowsStream?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

