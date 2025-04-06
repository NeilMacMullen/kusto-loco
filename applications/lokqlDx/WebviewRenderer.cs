using System.Windows;
using System.Windows.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.ScottPlotRendering;
using Lokql.Engine.Commands;
using NotNullStrings;
using ScottPlot;
using ScottPlot.WPF;
using Label = System.Windows.Controls.Label;

namespace lokqlDx;

public class WebViewRenderer(
    TabControl tabControl,
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
        await SafeInvoke(() =>
        {
            ScottPlotter.Render(Plotter, result, settings);
            return Task.FromResult(true);
        });
    }


    /// <summary>
    ///     Renders the result to an image using a headless webview
    /// </summary>
    public async Task<byte[]> RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        var plot = new Plot();
        await SafeInvoke(() =>
        {
            GenericScottPlotter.Render(plot, result, settings);
            return Task.FromResult(true);
        });
        var bytes = plot.GetImageBytes((int)pWidth, (int)pHeight, ImageFormat.Png);
        return bytes;
    }


    private async Task<bool> RenderResultToApplicationDisplay(KustoQueryResult result)
    {
        FillInDataGrid(result);
        tabControl.SelectedIndex = result.Visualization.ChartType.IsBlank() ? 0 : 1;

        return await Task.FromResult(true);
    }

    public async Task<T> SafeInvoke<T>(Func<Task<T>> func) => await Application.Current.Dispatcher.Invoke(func);


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
        for (var i = 0; i < dt.Columns.Count; i++)
        {
            var c = dt.Columns[i];
            c.ColumnName = c.ColumnName
                .Replace("_", "__");
        }

        dataGrid.ItemsSource = dt.DefaultView;
    }

    public readonly record struct UriOrHtml(string Uri, string Html);
}
