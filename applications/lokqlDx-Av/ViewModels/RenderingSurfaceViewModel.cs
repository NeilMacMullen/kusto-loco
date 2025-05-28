using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using KustoLoco.Core;
using KustoLoco.Core.Settings;
using KustoLoco.Rendering.ScottPlot;
using Lokql.Engine.Commands;
using lokqlDx;
using NotNullStrings;
using ScottPlot;

namespace LokqlDx.ViewModels;

public partial class RenderingSurfaceViewModel : ObservableObject, IResultRenderingSurface
{
    private readonly KustoSettingsProvider _kustoSettings;
    [ObservableProperty] private List<string> _columns = [];

    [ObservableProperty] private string _dataGridSizeWarning=string.Empty;
    [ObservableProperty] private double _fontSize = 20;


    private IScottPlotHost _plotter = new NullScottPlotHost();
    [ObservableProperty] private ObservableCollection<Row> _results = [];
    [ObservableProperty] private bool _showDataGridSizeWarning;

    public RenderingSurfaceViewModel(KustoSettingsProvider kustoSettings)
    {
        _kustoSettings = kustoSettings;
    }

    public async Task RenderToDisplay(KustoQueryResult result)
    {
        await RenderTable(result);
        var plot = _plotter.GetPlot(true);
        plot.Clear();
        ScottPlotKustoResultRenderer.RenderToPlot(plot, result, _kustoSettings);
        _plotter.FinishUpdate();
    }

    public byte[] RenderToImage(KustoQueryResult result, double pWidth, double pHeight)
    {
        using var plot = new Plot();
        ScottPlotKustoResultRenderer.RenderToPlot(plot, result, _kustoSettings);
        plot.Axes.AutoScale();
        var bytes = plot.GetImageBytes((int)pWidth, (int)pHeight, ImageFormat.Png);
        return bytes;
    }

    private Task RenderTable(KustoQueryResult result)
    {
        
        //ensure that if there are no results we clear the data grid
        if (result.Error.IsNotBlank())
        {
            Results = new ObservableCollection<Row>([new Row([result.Error])]);
            Columns = new List<string>(["ERROR"]);
            return Task.CompletedTask;
        }

        const int defaultMax = 10000;
        const string settingName = "datagrid.maxrows";
        var maxRows = _kustoSettings.GetIntOr(settingName, defaultMax);

        if (result.RowCount > maxRows)
            DataGridSizeWarning =
                $"Warning: Displaying only the first {maxRows} rows of {result.RowCount} rows.  Set {settingName} to see more";

        ShowDataGridSizeWarning = result.RowCount > maxRows;

        var rows = result.EnumerateRows()
            .Take(maxRows)
            .Select(row => new Row(row))
            .ToArray();
        
        Results = new ObservableCollection<Row>(rows);
        //note that changing Columns is the trigger for the view to redraw
        //soi we need to do this after the results are created
        Columns = result.ColumnNames().ToList();

        return Task.CompletedTask;
    }

    internal void SetUiPreferences(UIPreferences uiPreferences) => FontSize = uiPreferences.FontSize;

    internal void Clear() => Results?.Clear();

    public void RegisterHost(IScottPlotHost plotter) => _plotter = plotter;

    /// <summary>
    ///     Avalonia's DataGrid doesn't support binding to a DataTable so
    ///     create a fake row with indexer
    /// </summary>
    public class Row(object?[] rowItems)
    {
        public object? this[int index]
        {
            set => throw new InvalidOperationException();
            //it's possible to get transient mismatches during update so
            //just return a dummy value if the binding is temporarily invalid
            get => index >= rowItems.Length ? string.Empty : rowItems[index];
        }

        public static string GetPath(int i) => $"[{i}]";
    }
}
