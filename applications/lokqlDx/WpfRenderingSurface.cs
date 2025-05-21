using System.Windows;
using System.Windows.Controls;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using Lokql.Engine.Commands;
using NotNullStrings;
using ScottPlot;
using Label = System.Windows.Controls.Label;

namespace lokqlDx;

/// <summary>
///     Provides a rendering surface for the WPF lokqlDx application.
/// </summary>
public class WpfRenderingSurface(
    TabControl tabControl,
    DataGrid dataGrid,
    Label dataGridSizeWarning,
    IScottPlotHost plotter,
    KustoSettingsProvider settings)
    : IResultRenderingSurface
{
    public async Task RenderToDisplay(KustoQueryResult result)
    {
        await SafeInvoke(async () => await RenderResultToApplicationDisplay(result));
        await SafeInvoke(() =>
        {
            var plot = plotter.GetPlot(true);
            ScottPlotKustoResultRenderer.RenderToPlot(plot, result, settings);
            plotter.FinishUpdate();
            return Task.FromResult(true);
        });
    }


    /// <summary>
    ///     Renders the result to an image
    /// </summary>
    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        using var plot = new Plot();
        ScottPlotKustoResultRenderer.RenderToPlot(plot, result, settings);
        plot.Axes.AutoScale();
        var bytes = plot.GetImageBytes((int)pWidth, (int)pHeight, ImageFormat.Png);
        return bytes;
    }

    public void CopyToClipboard() => plotter.CopyToClipboard();

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

        const int defaultMax = 10000;
        const string settingName = "datagrid.maxrows";
        var maxRows = settings.GetIntOr(settingName, defaultMax);

        if (result.RowCount > maxRows)
            dataGridSizeWarning.Content =
                $"Warning: Displaying only the first {maxRows} rows of {result.RowCount} rows.  Set {settingName} to see more";
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
}
