using System.Windows;
using System.Windows.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering;
using Lokql.Engine.Commands;
using Microsoft.Web.WebView2.Wpf;
using NotNullStrings;
using ScottPlot.WPF;
using Label = System.Windows.Controls.Label;

namespace lokqlDx;

public class WebViewRenderer(
    TabControl tabControl,
    WebView2 webView,
    DataGrid dataGrid,
    Label dataGridSizeWarning,
    WpfPlot plotter,
    KustoSettingsProvider settings)
    : IResultRenderingSurface
{
    public WpfPlot Plotter { get; } = plotter;
    public UriOrHtml LastRendered { get; private set; } = new(string.Empty, string.Empty);

    public async Task RenderToDisplay(KustoQueryResult result)
    {
        await SafeInvoke(async () => await RenderResultToApplicationDisplay(result));
        await SafeInvoke(async () => await ScottPlotter.Render(Plotter,result));
    }

  

    /// <summary>
    ///     Renders the result to an image using a headless webview
    /// </summary>
    public async Task<byte[]> RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        var html = GetHtmlFromResult(result);
        return await SafeInvoke(async () => await WebViewExtensions.RenderToImage(html, pWidth, pHeight));
    }


    public async Task NavigateToUrl(Uri url)
    {
        await SafeInvoke(async () =>
        {
            LastRendered = new UriOrHtml(url.ToString(), string.Empty);
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Navigate(url.ToString());
            tabControl.SelectedIndex =1;
            return true;
        });
    }


    private string GetHtmlFromResult(KustoQueryResult result)
    {
        var renderer = new KustoResultRenderer(settings);
        return renderer.RenderToHtml(result);
    }

    private async Task<bool> RenderResultToApplicationDisplay(KustoQueryResult result)
    {
        var html = GetHtmlFromResult(result);
        //annoying we have to do this, but it's the only way to get the webview to render
        LastRendered = new UriOrHtml(string.Empty, html);
        await webView.EnsureCoreWebView2Async();
        await WebViewExtensions.NavigateToStringAsync(webView.CoreWebView2, html);
        FillInDataGrid(result);
        tabControl.SelectedIndex = result.Visualization.ChartType.IsBlank() ? 0 : 2;

        return true;
    }

    public async Task<T> SafeInvoke<T>(Func<Task<T>> func)
    {
        return await Application.Current.Dispatcher.Invoke(func);
    }


    private void FillInDataGrid(KustoQueryResult result)
    {
        //ensure that if there are no results we clear the data grid
        if (result.RowCount == 0)
        {
            dataGrid.ItemsSource = null;
            return;
        }

        var defaultMax = 10000;
        var settingName = "datagrid.maxrows";
        var maxRows = settings.GetIntOr(settingName, defaultMax);

        if (result.RowCount > maxRows)
            dataGridSizeWarning.Content =
                @$"Warning: Displaying only the first {maxRows} rows of {result.RowCount} rows.  Set {settingName} to see more";
        dataGridSizeWarning.Visibility =
            result.RowCount > maxRows ? Visibility.Visible : Visibility.Collapsed;
        var dt = result.ToDataTable(maxRows);
         //prevent the column names from being interpreted as hotkeys
        for (var i=0;i< dt.Columns.Count;i++)
        {
            var c = dt.Columns[i];
            c.ColumnName = c.ColumnName
                .Replace("_", "__");
        }
        dataGrid.ItemsSource = dt.DefaultView;

    }

    public readonly record struct UriOrHtml(string Uri, string Html);
}
