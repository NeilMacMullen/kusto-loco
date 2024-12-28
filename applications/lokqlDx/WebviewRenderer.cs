using System.Windows;
using System.Windows.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using Lokql.Engine.Commands;
using Microsoft.Web.WebView2.Wpf;

namespace lokqlDx;

public class WebViewRenderer : IResultRenderingSurface
{
    private readonly DataGrid _dataGrid;
    private readonly TextBox _visibleDataGridRows;
    private readonly Label _datagridSizeWarning;
    private readonly KustoSettingsProvider _settings;
    private readonly WebView2 _webView;

    public WebViewRenderer(WebView2 webView, DataGrid dataGrid,
        TextBox visibleDataGridRows,
        Label datagridSizeWarning,
        KustoSettingsProvider settings)
    {
        _webView = webView;
        _dataGrid = dataGrid;
        _visibleDataGridRows = visibleDataGridRows;
        _datagridSizeWarning = datagridSizeWarning;
        _settings = settings;
    }

    public void SetMaxVisibleDatagridRows(int n)
    {
        if (n < 1) n = 10000;
        _visibleDataGridRows.Text = n.ToString();
    }
    public int TryGetMaxVisibleDatagridRows()
    {
        var maxDataGridRows = int.TryParse(_visibleDataGridRows.Text, out var parsed)
            ? parsed
            : 10000;
        return maxDataGridRows;
    }

    public async Task RenderToDisplay(KustoQueryResult result)
    {
        await SafeInvoke(async () => await RenderResultToApplicationDisplay(result));
    }

    /// <summary>
    ///     Renders the result to an image using a headless webview
    /// </summary>
    public async Task<byte[]> RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        var html = GetHtmlFromResult(result);
        return await SafeInvoke(async () => { return await WebViewExtensions.RenderToImage(html, pWidth, pHeight); });
    }

    private string GetHtmlFromResult(KustoQueryResult result)
    {
        var renderer = new KustoResultRenderer(_settings);
        return renderer.RenderToHtml(result);
    }

    public async Task<bool> RenderResultToApplicationDisplay(KustoQueryResult result)
    {
        var html = GetHtmlFromResult(result);
        //annoying we have to do this, but it's the only way to get the webview to render
        await _webView.EnsureCoreWebView2Async();
        await WebViewExtensions.NavigateToStringAsync(_webView.CoreWebView2, html);
        FillInDataGrid(result);
        return true;
    }

    public async Task<T> SafeInvoke<T>(Func<Task<T>> func)
    {
        return await Application.Current.Dispatcher.Invoke(func);
    }


    private void FillInDataGrid(KustoQueryResult result)
    {
        //ensure that if have not results we clear the data grid
        if (result.RowCount == 0)
        {
            _dataGrid.ItemsSource = null;
            return;
        }

        var maxDataGridRows = TryGetMaxVisibleDatagridRows();

        _datagridSizeWarning.Visibility =
            result.RowCount > maxDataGridRows ? Visibility.Visible : Visibility.Collapsed;
        var dt = result.ToDataTable(maxDataGridRows);
        _dataGrid.ItemsSource = dt.DefaultView;
    }

}
