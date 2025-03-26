﻿using KustoLoco.Core;
using KustoLoco.Core.Evaluation;
using KustoLoco.Core.Settings;
using KustoLoco.Core.Util;
using KustoLoco.Rendering;
using Microsoft.Web.WebView2.Core;
using ScottPlot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Management.Automation;
using System.Runtime.InteropServices;
using Image = SixLabors.ImageSharp.Image;
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
                /*
                WriteObject("creating renderer");
               var renderer = new KustoResultRenderer(new KustoSettingsProvider());
               WriteObject("generating html");
                var html = renderer.RenderToHtml(result);
                WriteObject("rendering toimage");
                var res =  RenderToImage(html, 100, 100).GetAwaiter().GetResult();
                WriteObject("rendered to image");
                */
                double[] dataX = { 1, 2, 3, 4, 5 };
                double[] dataY = { 1, 4, 9, 16, 25 };

                ScottPlot.Plot myPlot = new();
                myPlot.Add.Scatter(dataX, dataY);
                byte[] bytes = myPlot.GetImageBytes(100,100,ImageFormat.Png);

                using MemoryStream strm = new(bytes);
              

               // var strm = new MemoryStream(res);
              var image = Image.Load<Rgba32>(strm);
              WriteObject("getting sixel...");
              var str = Sixel.ImageToSixel(image, 256, 20);
              WriteObject("sixel....");
              using var writer = new VTWriter();
              writer.Write(str);
              WriteObject(str);
            }
        }
    }

    public async Task<byte[]> RenderToImage(string html, double pWidth, double pHeight)
    {
       
        return await WebViewExtensions.RenderToImage(html, pWidth, pHeight);
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

public static class WebViewExtensions
{
    private static readonly IntPtr HWND_MESSAGE = new(-3);

    /// <summary>
    ///     Captures the image currently displayed in the webview.
    /// </summary>
    public static async Task<byte[]> CaptureImage(CoreWebView2 webview)
    {
        var stream = new MemoryStream();
        await webview.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, stream);

        return stream.GetBuffer();
    }

    public static async Task<byte[]> RenderToImage(string html, double pixelWidth, double pixelHeight)
    {
        var environment = await CoreWebView2Environment.CreateAsync();
        var browserController = await environment.CreateCoreWebView2ControllerAsync(HWND_MESSAGE);
        var bounds = new Rectangle(0, 0, (int)pixelWidth, (int)pixelHeight);
        browserController.Bounds = bounds;
        await NavigateToStringAsync(browserController.CoreWebView2, html);
        var image = await CaptureImage(browserController.CoreWebView2);
        browserController.Close();
        return image;
    }

    /// <summary>
    ///     Navigates to a string in the specified webView and waits for the navigation to complete.
    /// </summary>
    public static async Task NavigateToStringAsync(CoreWebView2 webView, string htmlContent, bool retry = false)
    {
        try
        {
            var tcs = new TaskCompletionSource<bool>();

            void NavigationCompletedHandler(object sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                webView.NavigationCompleted -= NavigationCompletedHandler!;
                tcs.SetResult(true);
            }

            webView.NavigationCompleted += NavigationCompletedHandler!;
            webView.NavigateToString(htmlContent);

            await tcs.Task;
        }
        catch
        {
            //sometimes we can't render content, for example if it's way too large, if so attempt to provide a warning
            if (!retry)
                await NavigateToStringAsync(webView, "<html><body><font color=\"red\">Unable to render content</font></body></html>", true);
        }
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

