using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using Lokql.Engine.Commands;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace lokqlDx;

public class WebviewRenderer : IResultRenderingSurface
{
    private readonly WebView2 _webview;
    private readonly DataGrid _dataGrid;
    private readonly KustoSettingsProvider _settings;
    private string _lastContent=string.Empty;
    private byte[] _image=[];

    public WebviewRenderer(WebView2 webview,DataGrid datagrid, KustoSettingsProvider settings)
    {
        _webview = webview;
        _dataGrid = datagrid;
        _settings = settings;
    }

    public async Task RenderResult(KustoQueryResult result)
    {
        var renderer = new KustoResultRenderer(_settings);
        var html = renderer.RenderToHtml(result);
        //annoying we have to do this, but it's the only way to get the webview to render
        await _webview.EnsureCoreWebView2Async();
        await NavigateToStringAsync(_webview.CoreWebView2,html);
        FillInDataGrid(result);
    }

    public async Task RenderToSurface(KustoQueryResult result)
    {
        await Application.Current.Dispatcher.Invoke(async () => { await RenderResult(result); });
    }
    static readonly IntPtr HWND_MESSAGE = new IntPtr( -3 );

    public async Task<byte[]> GetImage(double pWidth, double pHeight)
    {
      
       
        return await Application.Current.Dispatcher.Invoke(async () =>
        {

            var environment=await CoreWebView2Environment.CreateAsync();
            var browserController = await environment.CreateCoreWebView2ControllerAsync(HWND_MESSAGE);
            var bounds = new Rectangle(0, 0, (int)pWidth, (int)pHeight);
            browserController.Bounds = bounds;
            //browserController.SetBoundsAndZoomFactor(bounds,1);
             await NavigateToStringAsync(browserController.CoreWebView2, _lastContent);
          
             browserController.Close();
            File.WriteAllBytes(@$"C:\temp\debug_{(int)pWidth}x{(int)pHeight}.png",_image);
            return _image;
        });
        


    }

    public async Task NavigateToStringAsync(CoreWebView2 webview, string htmlContent)
    {
        _lastContent = htmlContent;
        var tcs = new TaskCompletionSource<bool>();

        void NavigationCompletedHandler(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            webview.NavigationCompleted -= NavigationCompletedHandler!;
            tcs.SetResult(true);
        }

        webview.NavigationCompleted += NavigationCompletedHandler!;
        webview.NavigateToString(htmlContent);

        await tcs.Task;

        var strm = new MemoryStream();
        await webview.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, strm);
        _image = strm.GetBuffer();
      

    }

 

    private void FillInDataGrid(KustoQueryResult result)
    {
        //ensure that if have not results we clear the data grid
        if (result.RowCount == 0)
        {
            _dataGrid.ItemsSource = null;
            return;
        }

        var dt = result.ToDataTable(10000);
        _dataGrid.ItemsSource = dt.DefaultView;
    }
}
